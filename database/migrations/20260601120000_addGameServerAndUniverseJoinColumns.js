/**
 * The game-server and universe join path reads columns that were referenced by the
 * application code but never created by an earlier migration. Without them the
 * PlaceLauncher/Join flow throws on every launch, so add them here.
 * @param {import('knex')} knex
 */
exports.up = async (knex) => {
    await knex.schema.alterTable('universe', (t) => {
        t.boolean('cloudedit').notNullable().defaultTo(false);
        t.integer('forcemorph_type').notNullable().defaultTo(1); // ForceMorphType.PlayerChoice
        t.integer('privacy_type').notNullable().defaultTo(1); // PrivacyType.Public
    });
    await knex.schema.alterTable('asset_server', (t) => {
        t.integer('status').notNullable().defaultTo(1); // ServerStatus.Loading
        t.integer('type').notNullable().defaultTo(1); // MatchmakingContextId.Default
        t.bigInteger('ping').notNullable().defaultTo(0);
        t.bigInteger('fps').notNullable().defaultTo(0);
    });
};

exports.down = async (knex) => {
    await knex.schema.alterTable('universe', (t) => {
        t.dropColumn('cloudedit');
        t.dropColumn('forcemorph_type');
        t.dropColumn('privacy_type');
    });
    await knex.schema.alterTable('asset_server', (t) => {
        t.dropColumn('status');
        t.dropColumn('type');
        t.dropColumn('ping');
        t.dropColumn('fps');
    });
};
