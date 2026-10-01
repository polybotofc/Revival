#!/usr/bin/env python3
"""Generate a consolidated PostgreSQL schema.sql from the legacy knex migrations.

The migrations use a small, regular subset of the knex schema builder. This script
walks them in timestamp order and emits equivalent DDL so the isolated web backend
can be provisioned without the original Node toolchain.
"""
import os
import re
import glob

MIGRATIONS = os.environ.get("MIGRATIONS", os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "migrations"))
OUT = os.environ.get("SCHEMA_OUT", os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "schema.sql"))

# ---------------------------------------------------------------- helpers

def strip_comments(text):
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"//[^\n]*", "", text)
    return text


def balanced(text, start):
    """Return the substring of a balanced { } block beginning at index start ('{')."""
    depth = 0
    i = start
    while i < len(text):
        c = text[i]
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                return text[start + 1:i]
        i += 1
    raise ValueError("unbalanced braces")


def find_close_paren(text, open_index):
    """Index of the ')' matching the '(' at open_index, quote-aware."""
    depth = 0
    quote = None
    i = open_index
    while i < len(text):
        c = text[i]
        if quote:
            if c == quote:
                quote = None
        elif c in "'\"":
            quote = c
        elif c == "(":
            depth += 1
        elif c == ")":
            depth -= 1
            if depth == 0:
                return i
        i += 1
    raise ValueError("unbalanced parens")


def parse_stmt(stmt):
    """Parse 'receiver.method(args).chain...' into (method, argstr, chain) or None."""
    m = re.match(r"^([A-Za-z_$][\w$]*)\.([A-Za-z]\w*)\s*\(", stmt)
    if not m:
        return None
    method = m.group(2)
    open_index = stmt.index("(", m.start(2))
    close = find_close_paren(stmt, open_index)
    args = stmt[open_index + 1:close]
    chain = stmt[close + 1:]
    return method, args, chain


def split_statements(body):
    """Split a table body into statements on top-level semicolons (quote-aware)."""
    out = []
    depth = 0
    cur = ""
    quote = None
    for c in body:
        if quote:
            cur += c
            if c == quote:
                quote = None
            continue
        if c in "'\"":
            quote = c
            cur += c
            continue
        if c in "([{":
            depth += 1
        elif c in ")]}":
            depth -= 1
        if c == ";" and depth == 0:
            out.append(cur.strip())
            cur = ""
        else:
            cur += c
    if cur.strip():
        out.append(cur.strip())
    return [s for s in out if s]


def split_args(argstr):
    """Split a function argument list on top-level commas."""
    out = []
    depth = 0
    cur = ""
    quote = None
    for c in argstr:
        if quote:
            cur += c
            if c == quote:
                quote = None
            continue
        if c in "'\"":
            quote = c
            cur += c
            continue
        if c in "([{":
            depth += 1
        elif c in ")]}":
            depth -= 1
        if c == "," and depth == 0:
            out.append(cur.strip())
            cur = ""
        else:
            cur += c
    if cur.strip():
        out.append(cur.strip())
    return out


def unquote(s):
    s = s.strip()
    if len(s) >= 2 and s[0] in "'\"" and s[-1] == s[0]:
        return s[1:-1]
    return s


# knex column method -> (postgres type, extra)
def column_type(method, args):
    if method in ("bigIncrements", "bigIncrements"):
        return "BIGSERIAL", {"pk": True}
    if method == "increments":
        return "SERIAL", {"pk": True}
    if method == "id":
        return "BIGSERIAL", {"pk": True}
    if method == "bigInteger":
        return "BIGINT", {}
    if method == "integer":
        return "INTEGER", {}
    if method == "smallint":
        return "SMALLINT", {}
    if method == "string":
        length = unquote(args[1]) if len(args) > 1 else "255"
        return "VARCHAR(%s)" % length, {}
    if method == "text":
        return "TEXT", {}
    if method in ("boolean", "bool"):
        return "BOOLEAN", {}
    if method == "double":
        return "DOUBLE PRECISION", {}
    if method == "float":
        return "REAL", {}
    if method == "decimal":
        p = unquote(args[1]) if len(args) > 1 else "8"
        s = unquote(args[2]) if len(args) > 2 else "2"
        return "NUMERIC(%s,%s)" % (p, s), {}
    if method == "dateTime":
        return "TIMESTAMPTZ", {}
    if method == "timestamp":
        tz = "useTz" in args[1] if len(args) > 1 else False
        return ("TIMESTAMPTZ" if tz else "TIMESTAMP"), {}
    if method == "uuid":
        return "UUID", {}
    if method == "specificType":
        return unquote(args[1]), {}
    if method in ("json", "jsonb"):
        return method.upper(), {}
    if method == "binary":
        return "BYTEA", {}
    raise KeyError(method)


def render_default(expr):
    expr = expr.strip()
    if "knex.fn.now()" in expr or "fn.now()" in expr:
        return "NOW()"
    if expr in ("null", "undefined"):
        return "NULL"
    if expr == "true":
        return "true"
    if expr == "false":
        return "false"
    if re.fullmatch(r"-?\d+(\.\d+)?", expr):
        return expr
    return "'%s'" % unquote(expr).replace("'", "''")


def parse_chain(chain):
    """Parse the trailing .foo(...) calls into a dict of flags/values."""
    flags = {"notNullable": False, "nullable": False, "primary": False,
             "unique": False, "index": False, "alter": False,
             "unsigned": False, "default": None, "references": None,
             "inTable": None, "onDelete": None}
    i = 0
    while i < len(chain):
        m = re.match(r"\s*\.(\w+)\s*\(", chain[i:])
        if not m:
            m2 = re.match(r"\s*\.(\w+)", chain[i:])
            if not m2:
                i += 1
                continue
            if m2.group(1) in flags:
                flags[m2.group(1)] = True
            i += m2.end()
            continue
        name = m.group(1)
        open_index = i + m.end() - 1
        close = find_close_paren(chain, open_index)
        arg = chain[open_index + 1:close]
        i = close + 1
        if name == "defaultTo":
            flags["default"] = arg
        elif name == "references":
            flags["references"] = unquote(arg)
        elif name == "inTable":
            flags["inTable"] = unquote(arg)
        elif name == "onDelete":
            flags["onDelete"] = unquote(arg)
        elif name in flags:
            flags[name] = True
    return flags


def render_column(name, method, args, chain, flags):
    if method == "dropColumn":
        return "ALTER TABLE {t} DROP COLUMN IF EXISTS {c};"
    ctype, extra = column_type(method, args)
    parts = ['"%s" %s' % (name, ctype)]
    if flags["primary"] or extra.get("pk"):
        parts.append("PRIMARY KEY")
    if flags["notNullable"]:
        parts.append("NOT NULL")
    if flags["default"] is not None:
        parts.append("DEFAULT %s" % render_default(flags["default"]))
    if flags["references"] and flags["inTable"]:
        ref = 'REFERENCES "%s" ("%s")' % (flags["inTable"], flags["references"])
        if flags["onDelete"]:
            ref += " ON DELETE %s" % flags["onDelete"]
        parts.append(ref)
    return " ".join(parts)


def parse_table_body(body):
    """Return (columns, table_constraints, post_statements)."""
    columns = []
    constraints = []
    post = []
    for stmt in split_statements(body):
        parsed = parse_stmt(stmt)
        if not parsed:
            continue
        method, argstr, chain = parsed
        args = split_args(argstr)
        flags = parse_chain(chain)
        if method in ("primary", "unique", "index"):
            arr = re.search(r"\[(.*?)\]", argstr, re.S)
            arrtext = arr.group(1) if arr else argstr
            cols = re.findall(r"'([^']+)'", arrtext) or re.findall(r'"([^"]+)"', arrtext)
            if method == "primary":
                constraints.append("PRIMARY KEY (%s)" % ", ".join('"%s"' % c for c in cols))
            elif method == "unique":
                constraints.append("UNIQUE (%s)" % ", ".join('"%s"' % c for c in cols))
            else:
                # t.index(['a','b']) or t.index(['a','b'], 'name')
                index_name = args[1] if len(args) > 1 and not args[1].strip().startswith("[") else None
                named = " \"%s\"" % unquote(index_name) if index_name else ""
                post.append("CREATE INDEX%s ON {table} (%s);" % (
                    named, ", ".join('"%s"' % c for c in cols)))
            continue
        if method in ("dropColumn", "dropIndex"):
            continue
        if not args:
            continue
        name = unquote(args[0])
        try:
            columns.append(render_column(name, method, args, chain, flags))
        except KeyError:
            pass
    return columns, constraints, post


# ---------------------------------------------------------------- main

files = sorted(glob.glob(os.path.join(MIGRATIONS, "*.js")))
out = []
out.append("-- Consolidated PostgreSQL schema for the Korone web backend.")
out.append("-- Generated from database/migrations/*.js (knex). Regenerate after adding migrations.")
out.append("-- Requires PostgreSQL. Migrations were originally authored for Postgres.")
out.append("")
out.append("BEGIN;")
out.append("")

indexes = []
created = set()

for path in files:
    raw = open(path, encoding="utf-8").read()
    text = strip_comments(raw)
    # Only the forward migration matters for a fresh schema; exports.down would
    # otherwise re-drop tables and re-add columns that up already handles.
    up = re.search(r"exports\.up\s*=\s*(?:async\s*)?(?:function\s*)?\(", text)
    if up:
        open_index = text.index("(", up.end() - 1)
        close = find_close_paren(text, open_index)
        brace = text.index("{", close)
        text = balanced(text, brace)

    # Collect schema operations in source order so create/alter/drop interleave
    # exactly as the migrations intend.
    ops = []
    for m in re.finditer(r"createTable\(\s*['\"]([^'\"]+)['\"]\s*,\s*(?:async\s*)?\(?\s*([A-Za-z_$][\w$]*)\s*\)?\s*=>\s*\{", text):
        ops.append((m.start(), "create", m.group(1), balanced(text, m.end() - 1)))
    for m in re.finditer(r"alterTable\(\s*['\"]([^'\"]+)['\"]\s*,\s*(?:async\s*)?\(?\s*([A-Za-z_$][\w$]*)\s*\)?\s*=>\s*\{", text):
        ops.append((m.start(), "alter", m.group(1), balanced(text, m.end() - 1)))
    for m in re.finditer(r"dropTableIfExists\(\s*['\"]([^'\"]+)['\"]\s*\)", text):
        ops.append((m.start(), "drop", m.group(1), None))
    for m in re.finditer(r"dropTable\(\s*['\"]([^'\"]+)['\"]\s*\)", text):
        ops.append((m.start(), "drop", m.group(1), None))
    ops.sort(key=lambda o: o[0])

    for _, kind, table, body in ops:
        if kind == "create":
            columns, constraints, post = parse_table_body(body)
            created.add(table)
            out.append("CREATE TABLE IF NOT EXISTS \"%s\" (" % table)
            lines = ["  " + c for c in columns] + ["  " + c for c in constraints]
            out.append(",\n".join(lines))
            out.append(");")
            out.append("")
            for p in post:
                indexes.append(p.replace("{table}", '"%s"' % table))
            continue

        if kind == "drop":
            out.append("DROP TABLE IF EXISTS \"%s\";" % table)
            continue

        # kind == "alter"
        for stmt in split_statements(body):
            parsed = parse_stmt(stmt)
            if not parsed:
                continue
            method, argstr, chain = parsed
            args = split_args(argstr)
            flags = parse_chain(chain)
            if method == "dropColumn":
                out.append('ALTER TABLE "%s" DROP COLUMN IF EXISTS "%s";' % (table, unquote(args[0])))
                continue
            if method == "dropIndex":
                cols = re.findall(r"'([^']+)'", argstr)
                name = unquote(args[-1]) if args and not args[-1].startswith("[") else None
                if name:
                    out.append('DROP INDEX IF EXISTS "%s";' % name)
                continue
            if method in ("primary", "unique", "index"):
                continue
            if not args:
                continue
            name = unquote(args[0])
            try:
                col = render_column(name, method, args, chain, flags)
            except KeyError:
                continue
            if flags["alter"]:
                ctype = column_type(method, args)[0]
                out.append('ALTER TABLE "%s" ALTER COLUMN "%s" TYPE %s;' % (table, name, ctype))
                out.append('ALTER TABLE "%s" ALTER COLUMN "%s" %s NOT NULL;' % (
                    table, name, "SET" if flags["notNullable"] else "DROP"))
                if flags["default"] is not None:
                    out.append('ALTER TABLE "%s" ALTER COLUMN "%s" SET DEFAULT %s;' % (
                        table, name, render_default(flags["default"])))
            else:
                out.append('ALTER TABLE "%s" ADD COLUMN IF NOT EXISTS %s;' % (table, col))
    # raw CREATE INDEX
    for m in re.finditer(r"knex\.raw\(\s*[`'\"]([^`'\"]*CREATE INDEX[^`'\"]*)[`'\"]", text, re.I):
        indexes.append(m.group(1).strip().rstrip(";") + ";")

out.append("")
out.append("-- Indexes")
out.extend(indexes)
out.append("")
out.append("COMMIT;")
out.append("")

open(OUT, "w", encoding="utf-8").write("\n".join(out))
print("wrote", OUT, "tables:", len(created), "indexes:", len(indexes))
print(sorted(created))
