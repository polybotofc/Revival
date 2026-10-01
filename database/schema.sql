-- Consolidated PostgreSQL schema for the Korone web backend.
-- Generated from database/migrations/*.js (knex). Regenerate after adding migrations.
-- Requires PostgreSQL. Migrations were originally authored for Postgres.

BEGIN;

CREATE TABLE IF NOT EXISTS "user" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "username" VARCHAR(64) NOT NULL,
  "password" VARCHAR(255) NOT NULL,
  "status" INTEGER NOT NULL DEFAULT 1,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "description" VARCHAR(1024) DEFAULT NULL,
  "online_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("username")
);

CREATE TABLE IF NOT EXISTS "user_email" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "email" VARCHAR(255) NOT NULL,
  "status" INTEGER NOT NULL DEFAULT 1,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_settings" (
  "user_id" BIGINT NOT NULL,
  "inventory_privacy" INTEGER NOT NULL DEFAULT 1,
  "theme" INTEGER NOT NULL DEFAULT 1,
  "gender" INTEGER NOT NULL DEFAULT 3,
  "birthday" TIMESTAMPTZ NOT NULL,
  UNIQUE ("user_id")
);

CREATE TABLE IF NOT EXISTS "user_avatar" (
  "user_id" BIGINT NOT NULL,
  "thumbnail_url" VARCHAR(255),
  "avatar_type" INTEGER NOT NULL DEFAULT 1,
  "scale_height" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_width" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_head" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_depth" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_proportion" DOUBLE PRECISION NOT NULL DEFAULT 0,
  "scale_body_type" DOUBLE PRECISION NOT NULL DEFAULT 0,
  "head_color_id" INTEGER NOT NULL,
  "torso_color_id" INTEGER NOT NULL,
  "right_arm_color_id" INTEGER NOT NULL,
  "left_arm_color_id" INTEGER NOT NULL,
  "right_leg_color_id" INTEGER NOT NULL,
  "left_leg_color_id" INTEGER NOT NULL,
  UNIQUE ("user_id")
);

CREATE TABLE IF NOT EXISTS "user_avatar_asset" (
  "user_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL
);

CREATE TABLE IF NOT EXISTS "user_economy" (
  "user_id" BIGINT NOT NULL,
  "balance_robux" INTEGER NOT NULL,
  "balance_tickets" INTEGER NOT NULL,
  UNIQUE ("user_id")
);

CREATE TABLE IF NOT EXISTS "user_asset" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "serial" BIGINT DEFAULT NULL,
  "price" BIGINT NOT NULL DEFAULT 0,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "catalog_overrides" (
  "asset_id" BIGINT NOT NULL,
  "is_limited" BOOLEAN NOT NULL DEFAULT false,
  "is_for_sale" BOOLEAN NOT NULL DEFAULT false,
  "copies" BIGINT DEFAULT NULL,
  "is_serialed" BOOLEAN NOT NULL DEFAULT false,
  "price" BIGINT DEFAULT 0,
  "offsale_at" TIMESTAMPTZ DEFAULT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("asset_id")
);

CREATE TABLE IF NOT EXISTS "user_badge" (
  "user_id" BIGINT NOT NULL,
  "badge_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_transaction" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "type" INTEGER NOT NULL,
  "currency_type" INTEGER NOT NULL,
  "amount" BIGINT NOT NULL,
  "details_json" VARCHAR(255) NOT NULL DEFAULT '{}',
  "user_id_one" BIGINT NOT NULL,
  "user_id_two" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "collectible_sale_logs" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "amount" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE "catalog_overrides" ADD COLUMN IF NOT EXISTS "rap" BIGINT NOT NULL DEFAULT 0;
CREATE TABLE IF NOT EXISTS "user_outfit" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "name" VARCHAR(255) NOT NULL,
  "user_id" BIGINT NOT NULL,
  "thumbnail_url" VARCHAR(255),
  "avatar_type" INTEGER NOT NULL DEFAULT 1,
  "scale_height" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_width" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_head" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_depth" DOUBLE PRECISION NOT NULL DEFAULT 1,
  "scale_proportion" DOUBLE PRECISION NOT NULL DEFAULT 0,
  "scale_body_type" DOUBLE PRECISION NOT NULL DEFAULT 0,
  "head_color_id" INTEGER NOT NULL,
  "torso_color_id" INTEGER NOT NULL,
  "right_arm_color_id" INTEGER NOT NULL,
  "left_arm_color_id" INTEGER NOT NULL,
  "right_leg_color_id" INTEGER NOT NULL,
  "left_leg_color_id" INTEGER NOT NULL,
  UNIQUE ("id")
);

CREATE TABLE IF NOT EXISTS "user_outfit_asset" (
  "outfit_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL
);

ALTER TABLE "user_settings" ADD COLUMN IF NOT EXISTS "trade_privacy" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "user_settings" ADD COLUMN IF NOT EXISTS "trade_filter" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "user_settings" ADD COLUMN IF NOT EXISTS "private_message_privacy" INTEGER NOT NULL DEFAULT 1;
CREATE TABLE IF NOT EXISTS "user_message" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id_from" BIGINT NOT NULL,
  "user_id_to" BIGINT NOT NULL,
  "is_read" BOOLEAN,
  "subject" VARCHAR(255) NOT NULL,
  "body" VARCHAR(1024) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "is_archived" BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS "user_friend" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id_one" BIGINT NOT NULL,
  "user_id_two" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_friend_request" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id_one" BIGINT NOT NULL,
  "user_id_two" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_previous_username" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "username" VARCHAR(255) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_ban" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "author_user_id" BIGINT NOT NULL,
  "reason" VARCHAR(255) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_status" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "status" VARCHAR(255),
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_following" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id_being_followed" BIGINT NOT NULL,
  "user_id_who_is_following" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_trade" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id_one" BIGINT NOT NULL,
  "user_id_two" BIGINT NOT NULL,
  "user_id_one_robux" BIGINT DEFAULT NULL,
  "user_id_two_robux" BIGINT DEFAULT NULL,
  "status" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "expires_at" TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS "user_trade_asset" (
  "trade_id" BIGINT NOT NULL,
  "user_asset_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL
);

CREATE TABLE IF NOT EXISTS "asset_comment" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "comment" VARCHAR(1024) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "forum_post" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "post" VARCHAR(1024) NOT NULL,
  "title" VARCHAR(255) DEFAULT NULL,
  "thread_id" BIGINT DEFAULT NULL,
  "sub_category_id" INTEGER NOT NULL,
  "is_pinned" BOOLEAN NOT NULL DEFAULT false,
  "is_locked" BOOLEAN NOT NULL DEFAULT false,
  "views" BIGINT NOT NULL DEFAULT 0,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "forum_post_read" (
  "forum_post_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  UNIQUE ("forum_post_id", "user_id")
);

CREATE TABLE IF NOT EXISTS "moderation_give_item" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "author_user_id" BIGINT NOT NULL,
  "user_asset_id" BIGINT NOT NULL,
  "user_id_from" BIGINT DEFAULT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "moderation_give_robux" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "author_user_id" BIGINT NOT NULL,
  "amount" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE "user" ADD COLUMN IF NOT EXISTS "session_key" INTEGER NOT NULL DEFAULT 0;
ALTER TABLE "user_transaction" ADD COLUMN IF NOT EXISTS "asset_id" BIGINT DEFAULT NULL;
ALTER TABLE "user_transaction" ADD COLUMN IF NOT EXISTS "user_asset_id" BIGINT DEFAULT NULL;
ALTER TABLE "user_transaction" ADD COLUMN IF NOT EXISTS "sub_type" INTEGER DEFAULT NULL;
ALTER TABLE "user_transaction" ADD COLUMN IF NOT EXISTS "old_username" VARCHAR(255) DEFAULT NULL;
ALTER TABLE "user_transaction" ADD COLUMN IF NOT EXISTS "new_username" VARCHAR(255) DEFAULT NULL;
ALTER TABLE "user_transaction" DROP COLUMN IF EXISTS "details_json";
ALTER TABLE "moderation_give_item" ALTER COLUMN "user_id" TYPE BIGINT;
ALTER TABLE "moderation_give_item" ALTER COLUMN "user_id" SET NOT NULL;
ALTER TABLE "moderation_give_item" ALTER COLUMN "author_user_id" TYPE BIGINT;
ALTER TABLE "moderation_give_item" ALTER COLUMN "author_user_id" SET NOT NULL;
ALTER TABLE "moderation_give_item" ALTER COLUMN "user_asset_id" TYPE BIGINT;
ALTER TABLE "moderation_give_item" ALTER COLUMN "user_asset_id" SET NOT NULL;
ALTER TABLE "moderation_give_item" ALTER COLUMN "user_id_from" TYPE BIGINT;
ALTER TABLE "moderation_give_item" ALTER COLUMN "user_id_from" DROP NOT NULL;
ALTER TABLE "moderation_give_item" ALTER COLUMN "user_id_from" SET DEFAULT NULL;
ALTER TABLE "moderation_give_robux" ALTER COLUMN "user_id" TYPE BIGINT;
ALTER TABLE "moderation_give_robux" ALTER COLUMN "user_id" SET NOT NULL;
ALTER TABLE "moderation_give_robux" ALTER COLUMN "author_user_id" TYPE BIGINT;
ALTER TABLE "moderation_give_robux" ALTER COLUMN "author_user_id" SET NOT NULL;
ALTER TABLE "moderation_give_robux" ALTER COLUMN "amount" TYPE BIGINT;
ALTER TABLE "moderation_give_robux" ALTER COLUMN "amount" SET NOT NULL;
ALTER TABLE "user_ban" ALTER COLUMN "user_id" TYPE BIGINT;
ALTER TABLE "user_ban" ALTER COLUMN "user_id" SET NOT NULL;
ALTER TABLE "user_ban" ALTER COLUMN "author_user_id" TYPE BIGINT;
ALTER TABLE "user_ban" ALTER COLUMN "author_user_id" SET NOT NULL;
ALTER TABLE "user_settings" DROP COLUMN IF EXISTS "birthday";
CREATE TABLE IF NOT EXISTS "group" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT,
  "locked" BOOLEAN NOT NULL DEFAULT false,
  "name" VARCHAR(255) NOT NULL,
  "description" VARCHAR(1024) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "group_settings" (
  "group_id" BIGINT NOT NULL,
  "approval_required" BOOLEAN NOT NULL DEFAULT false,
  "enemies_allowed" BOOLEAN NOT NULL DEFAULT false,
  "funds_visible" BOOLEAN NOT NULL DEFAULT false,
  "games_visible" BOOLEAN NOT NULL DEFAULT false,
  UNIQUE ("group_id")
);

CREATE TABLE IF NOT EXISTS "group_icon" (
  "group_id" BIGINT NOT NULL,
  "name" VARCHAR(255) NOT NULL,
  "is_approved" INTEGER NOT NULL DEFAULT 0,
  "user_id" BIGINT,
  UNIQUE ("group_id")
);

CREATE TABLE IF NOT EXISTS "group_role" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "group_id" BIGINT NOT NULL,
  "name" VARCHAR(255) NOT NULL,
  "description" VARCHAR(255) NOT NULL,
  "rank" INTEGER NOT NULL,
  "member_count" BIGINT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS "group_role_permission" (
  "group_role_id" BIGINT NOT NULL,
  "delete_from_wall" BOOLEAN NOT NULL DEFAULT false,
  "post_to_wall" BOOLEAN NOT NULL DEFAULT false,
  "invite_members" BOOLEAN NOT NULL DEFAULT false,
  "post_to_status" BOOLEAN NOT NULL DEFAULT false,
  "remove_members" BOOLEAN NOT NULL DEFAULT false,
  "view_status" BOOLEAN NOT NULL DEFAULT false,
  "view_wall" BOOLEAN NOT NULL DEFAULT false,
  "change_rank" BOOLEAN NOT NULL DEFAULT false,
  "advertise_group" BOOLEAN NOT NULL DEFAULT false,
  "manage_relationships" BOOLEAN NOT NULL DEFAULT false,
  "add_group_places" BOOLEAN NOT NULL DEFAULT false,
  "view_audit_logs" BOOLEAN NOT NULL DEFAULT false,
  "create_items" BOOLEAN NOT NULL DEFAULT false,
  "manage_items" BOOLEAN NOT NULL DEFAULT false,
  "spend_group_funds" BOOLEAN NOT NULL DEFAULT false,
  "manage_clan" BOOLEAN NOT NULL DEFAULT false,
  "manage_group_games" BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS "group_user" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "group_role_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL
);

CREATE TABLE IF NOT EXISTS "group_status" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "group_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "status" VARCHAR(255),
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "group_wall" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "group_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "content" VARCHAR(1024) NOT NULL,
  "is_deleted" BOOLEAN NOT NULL DEFAULT false,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "group_audit_log" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "group_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "action" INTEGER NOT NULL,
  "new_owner_user_id" BIGINT DEFAULT NULL,
  "old_role_id" BIGINT DEFAULT NULL,
  "new_role_id" BIGINT DEFAULT NULL,
  "user_id_range_change" BIGINT DEFAULT NULL,
  "role_set_id" BIGINT DEFAULT NULL,
  "old_rank" INTEGER DEFAULT NULL,
  "new_rank" INTEGER DEFAULT NULL,
  "old_name" VARCHAR(255) DEFAULT NULL,
  "new_name" VARCHAR(255) DEFAULT NULL,
  "old_description" VARCHAR(255) DEFAULT NULL,
  "new_description" VARCHAR(255) DEFAULT NULL,
  "post_desc" VARCHAR(255) DEFAULT NULL,
  "post_user_id" BIGINT DEFAULT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "group_social_link" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "group_id" BIGINT NOT NULL,
  "type" INTEGER NOT NULL,
  "url" VARCHAR(255) NOT NULL,
  "title" VARCHAR(255) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

DROP TABLE IF EXISTS "catalog_overrides";
CREATE TABLE IF NOT EXISTS "asset" (
  "roblox_asset_id" BIGINT DEFAULT NULL,
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "name" VARCHAR(255) NOT NULL,
  "description" VARCHAR(4096) DEFAULT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "asset_type" SMALLINT NOT NULL,
  "asset_genre" SMALLINT NOT NULL,
  "creator_type" SMALLINT NOT NULL,
  "creator_id" BIGINT NOT NULL,
  "moderation_status" SMALLINT NOT NULL,
  "is_for_sale" BOOLEAN NOT NULL DEFAULT false,
  "price_robux" BIGINT DEFAULT NULL,
  "price_tix" BIGINT DEFAULT NULL,
  "is_limited" BOOLEAN NOT NULL DEFAULT false,
  "is_limited_unique" BOOLEAN NOT NULL DEFAULT false,
  "serial_count" BIGINT DEFAULT NULL,
  "sale_count" BIGINT NOT NULL DEFAULT 0,
  "offsale_at" TIMESTAMPTZ DEFAULT NULL
);

CREATE TABLE IF NOT EXISTS "asset_version" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "version_number" INTEGER NOT NULL,
  "content_url" VARCHAR(512) NOT NULL,
  "creator_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "asset_thumbnail" (
  "asset_id" BIGINT NOT NULL,
  "asset_version_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "content_url" VARCHAR(512) NOT NULL,
  "moderation_status" SMALLINT NOT NULL
);

ALTER TABLE "user_avatar" ADD COLUMN IF NOT EXISTS "headshot_thumbnail_url" VARCHAR(255) DEFAULT NULL;
ALTER TABLE "asset_version" ALTER COLUMN "content_url" TYPE VARCHAR(512);
ALTER TABLE "asset_version" ALTER COLUMN "content_url" DROP NOT NULL;
ALTER TABLE "asset_version" ALTER COLUMN "content_url" SET DEFAULT NULL;
ALTER TABLE "asset_version" ADD COLUMN IF NOT EXISTS "content_id" BIGINT DEFAULT NULL;
ALTER TABLE "user_outfit" ADD COLUMN IF NOT EXISTS "headshot_thumbnail_url" VARCHAR(255);
ALTER TABLE "asset" ADD COLUMN IF NOT EXISTS "recent_average_price" BIGINT DEFAULT NULL;
ALTER TABLE "asset" ADD COLUMN IF NOT EXISTS "comments_enabled" BOOLEAN NOT NULL DEFAULT false;
CREATE TABLE IF NOT EXISTS "asset_media" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_type" INTEGER NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "media_asset_id" BIGINT DEFAULT NULL,
  "media_video_hash" VARCHAR(128) DEFAULT NULL,
  "media_video_title" VARCHAR(128) DEFAULT NULL,
  "is_approved" BOOLEAN NOT NULL DEFAULT false,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "universe" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "root_asset_id" BIGINT NOT NULL,
  "is_public" BOOLEAN NOT NULL DEFAULT false,
  "creator_id" BIGINT NOT NULL,
  "creator_type" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "universe_asset" (
  "universe_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL
);

CREATE TABLE IF NOT EXISTS "asset_place" (
  "asset_id" BIGINT NOT NULL,
  "max_player_count" INTEGER NOT NULL DEFAULT 10,
  "server_fill_mode" INTEGER NOT NULL DEFAULT 1,
  "server_slot_size" INTEGER DEFAULT NULL,
  "is_vip_enabled" BOOLEAN NOT NULL DEFAULT false,
  "vip_price" INTEGER DEFAULT NULL,
  "is_public_domain" BOOLEAN NOT NULL DEFAULT false,
  "access" INTEGER NOT NULL DEFAULT 1,
  "visit_count" BIGINT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS "asset_server" (
  "id" UUID NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "ip" VARCHAR(255) NOT NULL,
  "port" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "asset_server_player" (
  "server_id" UUID NOT NULL,
  "user_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL
);

CREATE TABLE IF NOT EXISTS "asset_play_history" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "ended_at" TIMESTAMPTZ DEFAULT NULL
);

ALTER TABLE "asset_server" ADD COLUMN IF NOT EXISTS "server_connection" VARCHAR(255) NOT NULL;
CREATE TABLE IF NOT EXISTS "asset_icon" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "content_url" VARCHAR(512) NOT NULL,
  "moderation_status" SMALLINT NOT NULL
);

ALTER TABLE "user_outfit" ADD COLUMN IF NOT EXISTS "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW();
CREATE TABLE IF NOT EXISTS "asset_advertisement" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "target_id" BIGINT NOT NULL,
  "target_type" SMALLINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "advertisement_type" SMALLINT NOT NULL,
  "advertisement_asset_id" BIGINT NOT NULL,
  "name" VARCHAR(512) NOT NULL,
  "impressions_all" BIGINT NOT NULL DEFAULT 0,
  "clicks_all" BIGINT NOT NULL DEFAULT 0,
  "bid_amount_tix_all" BIGINT NOT NULL DEFAULT 0,
  "bid_amount_robux_all" BIGINT NOT NULL DEFAULT 0,
  "impressions_last_run" BIGINT NOT NULL DEFAULT 0,
  "clicks_last_run" BIGINT NOT NULL DEFAULT 0,
  "bid_amount_robux_last_run" BIGINT NOT NULL DEFAULT 0,
  "bid_amount_tix_last_run" BIGINT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS "moderation_bad_username" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "username" VARCHAR(512) NOT NULL
);

CREATE TABLE IF NOT EXISTS "moderation_bad_username_log" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "username" VARCHAR(512) NOT NULL,
  "user_id" BIGINT NOT NULL,
  "author_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "join_application" (
  "id" VARCHAR(128) NOT NULL,
  "preferred_name" VARCHAR(512) NOT NULL,
  "about" VARCHAR(4096) NOT NULL,
  "matrix_name" VARCHAR(512) NOT NULL,
  "matrix_domain" VARCHAR(512) NOT NULL,
  "social_presence" VARCHAR(512) NOT NULL,
  "user_id" BIGINT DEFAULT NULL,
  "author_id" BIGINT DEFAULT NULL,
  "reject_reason" VARCHAR(512) DEFAULT NULL,
  "status" INTEGER NOT NULL DEFAULT 1,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE "user" ADD COLUMN IF NOT EXISTS "is_18_plus" BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE "asset" ADD COLUMN IF NOT EXISTS "is_18_plus" BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "join_id" VARCHAR(128) DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "user_invite" (
  "id" VARCHAR(128) NOT NULL,
  "user_id" BIGINT DEFAULT NULL,
  "author_id" BIGINT DEFAULT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "asset_vote" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "type" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE "user_message" ALTER COLUMN "body" TYPE VARCHAR(8192);
ALTER TABLE "user_message" ALTER COLUMN "body" SET NOT NULL;
CREATE TABLE IF NOT EXISTS "moderation_give_tickets" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "author_user_id" BIGINT NOT NULL,
  "amount" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "trade_currency_order" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "start_amount" BIGINT NOT NULL,
  "balance" BIGINT NOT NULL,
  "exchange_rate" BIGINT NOT NULL,
  "source_currency" INTEGER NOT NULL,
  "destination_currency" INTEGER NOT NULL,
  "is_closed" BOOLEAN NOT NULL DEFAULT false,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "closed_at" TIMESTAMPTZ DEFAULT NULL
);

CREATE TABLE IF NOT EXISTS "trade_currency_log" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "order_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "source_amount" BIGINT NOT NULL,
  "destination_amount" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE "user_transaction" ADD COLUMN IF NOT EXISTS "group_id_one" BIGINT DEFAULT NULL;
ALTER TABLE "user_transaction" ADD COLUMN IF NOT EXISTS "group_id_two" BIGINT DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "group_economy" (
  "group_id" BIGINT NOT NULL,
  "balance_robux" INTEGER NOT NULL,
  "balance_tickets" INTEGER NOT NULL,
  UNIQUE ("group_id")
);

ALTER TABLE "user_ban" ADD COLUMN IF NOT EXISTS "expired_at" TIMESTAMPTZ DEFAULT NULL;
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "locked_at" TIMESTAMPTZ DEFAULT NULL;
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "locked_by_user_id" BIGINT DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "user_membership" (
  "user_id" BIGINT NOT NULL,
  "membership_type" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("user_id")
);

CREATE TABLE IF NOT EXISTS "moderation_change_join_app" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "application_id" VARCHAR(255) NOT NULL,
  "author_user_id" BIGINT NOT NULL,
  "new_status" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "is_verified" BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "verified_url" VARCHAR(512) DEFAULT NULL;
ALTER TABLE "join_application" DROP COLUMN IF EXISTS "matrix_name";
ALTER TABLE "join_application" DROP COLUMN IF EXISTS "matrix_domain";
ALTER TABLE "user_ban" ADD COLUMN IF NOT EXISTS "internal_reason" VARCHAR(4096) DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "asset_package" (
  "package_asset_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL,
  UNIQUE ("package_asset_id", "asset_id")
);

CREATE TABLE IF NOT EXISTS "abuse_report" (
  "id" VARCHAR(128) NOT NULL,
  "user_id" BIGINT NOT NULL,
  "author_id" BIGINT DEFAULT NULL,
  "report_reason" INTEGER NOT NULL,
  "report_status" INTEGER NOT NULL,
  "report_message" VARCHAR(1024) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("id")
);

ALTER TABLE "group_audit_log" ADD COLUMN IF NOT EXISTS "fund_recipient_user_id" BIGINT DEFAULT NULL;
ALTER TABLE "group_audit_log" ADD COLUMN IF NOT EXISTS "currency_amount" BIGINT DEFAULT NULL;
ALTER TABLE "group_audit_log" ADD COLUMN IF NOT EXISTS "currency_type" INTEGER DEFAULT NULL;
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "verified_id" VARCHAR(512) DEFAULT NULL;
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "verification_phrase" VARCHAR(512) DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "asset_favorite" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("user_id", "asset_id")
);

CREATE TABLE IF NOT EXISTS "asset_datastore" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "universe_id" BIGINT NOT NULL,
  "scope" VARCHAR(255) NOT NULL,
  "key" VARCHAR(255) NOT NULL,
  "name" VARCHAR(255) NOT NULL,
  "value" VARCHAR(1048576) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_permission" (
  "user_id" BIGINT NOT NULL,
  "permission" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("user_id", "permission")
);

CREATE TABLE IF NOT EXISTS "user_conversation" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "title" VARCHAR(255) DEFAULT NULL,
  "creator_id" BIGINT NOT NULL,
  "conversation_type" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_conversation_participant" (
  "conversation_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("conversation_id", "user_id")
);

CREATE TABLE IF NOT EXISTS "user_conversation_message" (
  "id" VARCHAR(64) NOT NULL,
  "conversation_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "message" TEXT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("id")
);

CREATE TABLE IF NOT EXISTS "user_conversation_message_read" (
  "conversation_id" BIGINT NOT NULL,
  "user_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("conversation_id", "user_id")
);

ALTER TABLE "user" ADD COLUMN IF NOT EXISTS "session_expired_at" TIMESTAMPTZ DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "moderation_manage_asset" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "action" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "moderation_ban" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "reason" VARCHAR(1024) NOT NULL,
  "internal_reason" VARCHAR(1024),
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "expired_at" TIMESTAMPTZ DEFAULT NULL
);

CREATE TABLE IF NOT EXISTS "moderation_unban" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "moderation_admin_message" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "subject" VARCHAR(1024) NOT NULL,
  "body" VARCHAR(4096) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "moderation_set_alert" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "alert" VARCHAR(4096) DEFAULT NULL,
  "alert_url" VARCHAR(4096) DEFAULT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_password_reset" (
  "id" VARCHAR(64) PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "status" INTEGER NOT NULL,
  "social_url" VARCHAR(1024) NOT NULL,
  "verification_phrase" VARCHAR(1024) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "asset_version_metadata_image" (
  "asset_version_id" BIGINT NOT NULL,
  "image_format" INTEGER NOT NULL,
  "resolution_x" INTEGER NOT NULL,
  "resolution_y" INTEGER NOT NULL,
  "size_bytes" INTEGER NOT NULL,
  "hash" VARCHAR(64) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "moderation_migrate_asset" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "roblox_asset_id" BIGINT NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "moderation_update_product" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "is_limited" BOOLEAN NOT NULL,
  "is_limited_unique" BOOLEAN NOT NULL,
  "is_for_sale" BOOLEAN NOT NULL,
  "price_in_robux" INTEGER,
  "price_in_tickets" INTEGER,
  "max_copies" INTEGER,
  "offsale_at" TIMESTAMPTZ,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "moderation_refund_transaction" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "transaction_id" BIGINT NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "user_id_one" BIGINT NOT NULL,
  "user_id_two" BIGINT NOT NULL,
  "asset_id" BIGINT,
  "user_asset_id" BIGINT,
  "amount" BIGINT NOT NULL,
  "currency_type" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_totp" (
  "user_id" BIGINT,
  "secret" TEXT,
  "status" INTEGER NOT NULL DEFAULT 2,
  PRIMARY KEY ("user_id")
);

ALTER TABLE "user" ADD COLUMN IF NOT EXISTS "discord_id" TEXT;
ALTER TABLE "user" ADD COLUMN IF NOT EXISTS "discordAuthCode" TEXT;
ALTER TABLE "user" ADD COLUMN IF NOT EXISTS "linkstatus" INTEGER NOT NULL DEFAULT 3;
ALTER TABLE "user" ADD COLUMN IF NOT EXISTS "verified" BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "discord_id" VARCHAR(512);
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "discord_username" VARCHAR(32);
ALTER TABLE "join_application" ADD COLUMN IF NOT EXISTS "reffered_by" INTEGER;
ALTER TABLE "asset_place" ADD COLUMN IF NOT EXISTS "year" BIGINT NOT NULL DEFAULT 2021;
ALTER TABLE "asset_place" ADD COLUMN IF NOT EXISTS "roblox_place_id" BIGINT NOT NULL DEFAULT 1818;
ALTER TABLE "user_settings" ADD COLUMN IF NOT EXISTS "join_privacy" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "user_settings" ADD COLUMN IF NOT EXISTS "avatar_page_style" INTEGER NOT NULL DEFAULT 1;
CREATE TABLE IF NOT EXISTS "user_connections" (
  "user_id" BIGINT PRIMARY KEY NOT NULL,
  "twitter" VARCHAR(15),
  "youtube" VARCHAR(30),
  "tiktok" VARCHAR(24),
  "discord" VARCHAR(32),
  "telegram" VARCHAR(32),
  "twitch" VARCHAR(25),
  "github" VARCHAR(39),
  "roblox" VARCHAR(20)
);

CREATE TABLE IF NOT EXISTS "asset_badge" (
  "asset_id" BIGINT,
  "universe_id" BIGINT,
  "enabled" BOOLEAN NOT NULL DEFAULT true
);

ALTER TABLE "user_avatar" ADD COLUMN IF NOT EXISTS "thumbnail_3d_url" VARCHAR(255) DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "user_mac_address" (
  "user_id" BIGINT NOT NULL REFERENCES "user" ("id") ON DELETE CASCADE,
  "mac_address" macaddr NOT NULL,
  "created_at" TIMESTAMPTZ DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ DEFAULT NOW(),
  PRIMARY KEY ("user_id", "mac_address")
);

CREATE TABLE IF NOT EXISTS "purchase_attestation_log" (
  "id" BIGSERIAL PRIMARY KEY,
  "user_id" BIGINT NOT NULL,
  "ticket_id" VARCHAR(32) NOT NULL,
  "asset_id" BIGINT,
  "expected_price" BIGINT,
  "outcome" SMALLINT NOT NULL,
  "client_ip_hash" VARCHAR(128),
  "user_agent" VARCHAR(512),
  "issued_at" TIMESTAMP NOT NULL DEFAULT NOW(),
  "consumed_at" TIMESTAMP,
  UNIQUE ("user_id", "ticket_id")
);

ALTER TABLE "universe" ADD COLUMN IF NOT EXISTS "topic" VARCHAR(280) DEFAULT NULL;
CREATE TABLE IF NOT EXISTS "user_game_recommendation" (
  "id" BIGSERIAL PRIMARY KEY NOT NULL,
  "user_id" BIGINT NOT NULL,
  "asset_id" BIGINT NOT NULL,
  "score" double precision NOT NULL,
  "position" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("user_id", "asset_id")
);

CREATE TABLE IF NOT EXISTS "ugc_request" (
  "id" BIGSERIAL PRIMARY KEY,
  "user_id" BIGINT NOT NULL,
  "roblox_asset_id" BIGINT NOT NULL,
  "roblox_url" VARCHAR(512) NOT NULL,
  "item_name" VARCHAR(256),
  "status" SMALLINT NOT NULL DEFAULT 0,
  "decided_by" BIGINT,
  "created_asset_id" BIGINT,
  "created_at" TIMESTAMP NOT NULL DEFAULT NOW(),
  "decided_at" TIMESTAMP
);

ALTER TABLE "universe" ADD COLUMN IF NOT EXISTS "cloudedit" BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE "universe" ADD COLUMN IF NOT EXISTS "forcemorph_type" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "universe" ADD COLUMN IF NOT EXISTS "privacy_type" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "asset_server" ADD COLUMN IF NOT EXISTS "status" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "asset_server" ADD COLUMN IF NOT EXISTS "type" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "asset_server" ADD COLUMN IF NOT EXISTS "ping" BIGINT NOT NULL DEFAULT 0;
ALTER TABLE "asset_server" ADD COLUMN IF NOT EXISTS "fps" BIGINT NOT NULL DEFAULT 0;
CREATE TABLE IF NOT EXISTS "donation_webhook_event" (
  "id" BIGSERIAL PRIMARY KEY,
  "provider" VARCHAR(32) NOT NULL,
  "external_event_id" VARCHAR(128) NOT NULL,
  "amount" NUMERIC(18,2) NOT NULL,
  "currency" VARCHAR(8) NOT NULL,
  "donor_display_name" VARCHAR(128),
  "user_id" BIGINT,
  "status" VARCHAR(32) NOT NULL,
  "skip_reason" VARCHAR(128),
  "created_at" TIMESTAMP NOT NULL DEFAULT NOW(),
  "processed_at" TIMESTAMP,
  UNIQUE ("provider", "external_event_id")
);

CREATE TABLE IF NOT EXISTS "moderation_rollback_trade" (
  "id" BIGSERIAL PRIMARY KEY,
  "trade_id" BIGINT NOT NULL,
  "actor_id" BIGINT NOT NULL,
  "user_id_one" BIGINT NOT NULL,
  "user_id_two" BIGINT NOT NULL,
  "created_at" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_machine_ban" (
  "user_id" BIGINT PRIMARY KEY REFERENCES "user" ("id") ON DELETE CASCADE,
  "actor_user_id" BIGINT REFERENCES "user" ("id") ON DELETE SET NULL,
  "internal_reason" VARCHAR(4096),
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "revoked_at" TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS "machine_ban_enforcement" (
  "user_id" BIGINT PRIMARY KEY REFERENCES "user" ("id") ON DELETE CASCADE,
  "source_user_id" BIGINT NOT NULL REFERENCES "user" ("id") ON DELETE CASCADE,
  "actor_user_id" BIGINT REFERENCES "user" ("id") ON DELETE SET NULL,
  "execute_at" TIMESTAMPTZ NOT NULL,
  "lease_until" TIMESTAMPTZ,
  "attempt_count" INTEGER NOT NULL DEFAULT 0,
  "last_error" VARCHAR(2048),
  "completed_at" TIMESTAMPTZ,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "user_ip_address" (
  "id" BIGSERIAL PRIMARY KEY,
  "user_id" BIGINT NOT NULL REFERENCES "user" ("id") ON DELETE CASCADE,
  "ip_hash" VARCHAR(128) NOT NULL,
  "action" VARCHAR(64) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE ("user_id", "ip_hash", "action")
);

CREATE TABLE IF NOT EXISTS "user_ip_ban" (
  "ip_hash" VARCHAR(128) PRIMARY KEY,
  "actor_user_id" BIGINT REFERENCES "user" ("id") ON DELETE SET NULL,
  "internal_reason" VARCHAR(4096) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "revoked_at" TIMESTAMPTZ
);


-- Indexes
CREATE INDEX ON "user_avatar_asset" ("user_id");
CREATE INDEX ON "user_asset" ("asset_id");
CREATE INDEX ON "user_asset" ("user_id");
CREATE INDEX ON "user_badge" ("user_id");
CREATE INDEX ON "user_transaction" ("user_id_one");
CREATE INDEX ON "user_transaction" ("user_id_two");
CREATE INDEX ON "collectible_sale_logs" ("asset_id");
CREATE INDEX ON "user_outfit" ("user_id");
CREATE INDEX ON "user_outfit_asset" ("outfit_id");
CREATE INDEX ON "user_message" ("user_id_from");
CREATE INDEX ON "user_message" ("user_id_to");
CREATE INDEX ON "user_friend" ("user_id_one");
CREATE INDEX ON "user_friend" ("user_id_two");
CREATE INDEX ON "user_friend_request" ("user_id_one");
CREATE INDEX ON "user_friend_request" ("user_id_two");
CREATE INDEX ON "user_previous_username" ("user_id");
CREATE INDEX ON "user_previous_username" ("username");
CREATE INDEX ON "user_ban" ("author_user_id");
CREATE INDEX ON "user_ban" ("user_id");
CREATE INDEX ON "user_status" ("user_id");
CREATE INDEX ON "user_following" ("user_id_being_followed");
CREATE INDEX ON "user_following" ("user_id_who_is_following");
CREATE INDEX ON "user_trade" ("user_id_one");
CREATE INDEX ON "user_trade" ("user_id_two");
CREATE INDEX ON "user_trade_asset" ("trade_id");
CREATE INDEX ON "asset_comment" ("asset_id", "id");
CREATE INDEX ON "asset_comment" ("user_id");
CREATE INDEX ON "forum_post" ("id");
CREATE INDEX ON "forum_post" ("thread_id", "id");
CREATE INDEX ON "forum_post" ("user_id", "created_at");
CREATE INDEX ON "forum_post" ("sub_category_id", "id");
CREATE INDEX  ON "user_asset" ("asset_id", "price") WHERE "price" > 0 AND "price" IS NOT NULL;
CREATE INDEX  ON "user_asset" ("asset_id");
CREATE INDEX  ON "user_asset" ("asset_id", "id");
CREATE INDEX  ON "user_transaction" ("user_asset_id") WHERE user_asset_id IS NOT NULL;
CREATE INDEX ON "group" ("user_id");
CREATE INDEX ON "group" ("name");
CREATE INDEX ON "group_role" ("group_id");
CREATE INDEX ON "group_user" ("group_role_id", "id");
CREATE INDEX ON "group_user" ("user_id");
CREATE INDEX ON "group_status" ("user_id");
CREATE INDEX "group_wall_id_idx" ON "group_wall" ("group_id", "id") WHERE "is_deleted" IS FALSE;
CREATE INDEX ON "group_social_link" ("group_id");
CREATE INDEX ON "asset" ("roblox_asset_id");
CREATE INDEX ON "asset" ("asset_type");
CREATE INDEX ON "asset" ("is_limited");
CREATE INDEX ON "asset_version" ("asset_id");
CREATE INDEX ON "asset_thumbnail" ("asset_id");
CREATE INDEX ON "universe_asset" ("universe_id");
CREATE INDEX ON "universe_asset" ("asset_id");
CREATE INDEX ON "asset_server" ("id");
CREATE INDEX ON "asset_server" ("asset_id");
CREATE INDEX ON "asset_server_player" ("asset_id");
CREATE INDEX ON "asset_server_player" ("server_id");
CREATE INDEX ON "asset_icon" ("asset_id");
CREATE INDEX ON "asset_advertisement" ("target_id", "target_type");
CREATE INDEX ON "asset_advertisement" ("updated_at");
CREATE INDEX ON "moderation_bad_username" ("username");
CREATE INDEX  ON "asset" ("creator_id", "creator_type") WHERE (is_for_sale OR is_limited);
CREATE INDEX  ON "asset" ("creator_id", "creator_type", "asset_type") WHERE (is_for_sale OR is_limited);
CREATE INDEX  ON "user_transaction" ("asset_id") WHERE (type = 1 AND sub_type = 1);
CREATE INDEX ON "asset_favorite" ("asset_id");
CREATE INDEX ON "asset_favorite" ("user_id");
CREATE INDEX IF NOT EXISTS "forum_post_thread_id" ON "forum_post" ("thread_id") WHERE (thread_id IS NOT NULL);
CREATE INDEX IF NOT EXISTS "forum_post_subcategory_id" ON "forum_post" ("sub_category_id");
CREATE INDEX IF NOT EXISTS "forum_post_subcategory_id_id_desc" ON "forum_post" ("sub_category_id", "id" desc);
CREATE INDEX ON "user_permission" ("user_id");
CREATE INDEX ON "user_conversation_participant" ("conversation_id");
CREATE INDEX ON "user_conversation_participant" ("user_id");
CREATE INDEX ON "user_conversation_message" ("conversation_id");
CREATE INDEX ON "user_conversation_message" ("conversation_id", "created_at");
CREATE INDEX ON "purchase_attestation_log" ("user_id", "issued_at");
CREATE INDEX ON "purchase_attestation_log" ("outcome", "issued_at");
CREATE INDEX "user_game_recommendation_user_idx" ON "user_game_recommendation" ("user_id", "position");
CREATE INDEX ON "ugc_request" ("user_id", "created_at");
CREATE INDEX ON "ugc_request" ("status", "created_at");
CREATE INDEX ON "donation_webhook_event" ("status", "created_at");
CREATE INDEX ON "donation_webhook_event" ("user_id", "created_at");
CREATE INDEX ON "moderation_rollback_trade" ("trade_id");
CREATE INDEX ON "moderation_rollback_trade" ("actor_id", "created_at");
CREATE INDEX "user_machine_ban_active_idx" ON "user_machine_ban" ("revoked_at", "user_id");
CREATE INDEX "machine_ban_enforcement_due_idx" ON "machine_ban_enforcement" ("completed_at", "execute_at");
CREATE INDEX "user_ip_address_hash_action_idx" ON "user_ip_address" ("ip_hash", "action");
CREATE INDEX "user_ip_ban_active_idx" ON "user_ip_ban" ("revoked_at", "ip_hash");

COMMIT;
