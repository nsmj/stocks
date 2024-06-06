# This file is auto-generated from the current state of the database. Instead
# of editing this file, please use the migrations feature of Active Record to
# incrementally modify your database, and then regenerate this schema definition.
#
# This file is the source Rails uses to define your schema when running `bin/rails
# db:schema:load`. When creating a new database, `bin/rails db:schema:load` tends to
# be faster and is potentially less error prone than running all of your
# migrations from scratch. Old migrations may fail to apply correctly if those
# migrations use external dependencies or application code.
#
# It's strongly recommended that you check this file into your version control system.

ActiveRecord::Schema[7.1].define(version: 2024_05_27_192357) do
  create_table "asset", force: :cascade do |t|
    t.text "code", null: false
    t.text "name", null: false
    t.text "note_description", null: false
    t.text "share_type"
    t.decimal "average_price"
    t.integer "position"
    t.text "cnpj"
    t.text "paying_source_cnpj"
    t.decimal "ceiling_price_factor"
    t.decimal "average_ceiling_price"
    t.decimal "projective_ceiling_price"
    t.integer "asset_type_id", null: false
  end

  create_table "asset_type", force: :cascade do |t|
    t.text "name", null: false
  end

  create_table "earning", force: :cascade do |t|
    t.decimal "value", null: false
    t.datetime "date", precision: nil, null: false
    t.integer "asset_id", null: false
  end

  create_table "end_year_position", force: :cascade do |t|
    t.integer "year", null: false
    t.decimal "average_price", null: false
    t.integer "position", null: false
    t.decimal "total_cost", null: false
    t.integer "asset_id", null: false
  end

  create_table "event", force: :cascade do |t|
    t.datetime "date", precision: nil, null: false
    t.integer "factor", null: false
    t.decimal "value"
    t.integer "event_type_id", null: false
    t.integer "asset_id", null: false
  end

  create_table "event_type", force: :cascade do |t|
    t.text "name", null: false
  end

  create_table "irrf", force: :cascade do |t|
    t.decimal "value", null: false
    t.datetime "date", precision: nil, null: false
    t.integer "trade_type_id", null: false
  end

  create_table "trade", force: :cascade do |t|
    t.datetime "date", precision: nil, null: false
    t.integer "quantity"
    t.decimal "asset_price"
    t.decimal "total_amount"
    t.decimal "fees"
    t.integer "purchase", null: false
    t.decimal "net_profit"
    t.integer "asset_id", null: false
    t.integer "trade_type_id", null: false
  end

  create_table "trade_type", force: :cascade do |t|
    t.text "name", null: false
  end

  add_foreign_key "asset", "asset_type"
  add_foreign_key "earning", "asset"
  add_foreign_key "end_year_position", "asset"
  add_foreign_key "event", "asset"
  add_foreign_key "event", "event_type"
  add_foreign_key "irrf", "trade_type"
  add_foreign_key "trade", "asset"
  add_foreign_key "trade", "trade_type"
end
