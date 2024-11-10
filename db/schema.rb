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
  create_table "ativo", force: :cascade do |t|
    t.text "codigo", null: false
    t.text "nome", null: false
    t.text "descricao_nota", null: false
    t.text "tipo_acao"
    t.decimal "preco_medio"
    t.integer "posicao"
    t.text "cnpj"
    t.text "cnpj_fonte_pagadora"
    t.decimal "fator_preco_teto"
    t.decimal "preco_teto_medio"
    t.decimal "preco_teto_projetivo"
    t.integer "tipo_ativo_id", null: false
  end

  create_table "evento", force: :cascade do |t|
    t.datetime "data", precision: nil, null: false
    t.integer "fator", null: false
    t.decimal "valor"
    t.integer "tipo_evento_id", null: false
    t.integer "ativo_id", null: false
  end

  create_table "irrf", force: :cascade do |t|
    t.decimal "valor", null: false
    t.datetime "data", precision: nil, null: false
    t.integer "tipo_operacao_id", null: false
  end

  create_table "operacao", force: :cascade do |t|
    t.datetime "data", precision: nil, null: false
    t.integer "quantidade"
    t.decimal "preco_ativo"
    t.decimal "valor_total"
    t.decimal "taxas"
    t.integer "compra", null: false
    t.decimal "lucro_liquido"
    t.integer "ativo_id", null: false
    t.integer "tipo_operacao_id", null: false
  end

  create_table "posicao_fim_ano", force: :cascade do |t|
    t.integer "ano", null: false
    t.decimal "preco_medio", null: false
    t.integer "posicao", null: false
    t.decimal "custo_total", null: false
    t.integer "ativo_id", null: false
  end

  create_table "rendimento", force: :cascade do |t|
    t.decimal "valor", null: false
    t.datetime "data", precision: nil, null: false
    t.integer "ativo_id", null: false
  end

  create_table "tipo_ativo", force: :cascade do |t|
    t.text "nome", null: false
  end

  create_table "tipo_evento", force: :cascade do |t|
    t.text "nome", null: false
  end

  create_table "tipo_operacao", force: :cascade do |t|
    t.text "nome", null: false
  end

  add_foreign_key "ativo", "tipo_ativo"
  add_foreign_key "evento", "ativo"
  add_foreign_key "evento", "tipo_evento"
  add_foreign_key "irrf", "tipo_operacao"
  add_foreign_key "operacao", "ativo"
  add_foreign_key "operacao", "tipo_operacao"
  add_foreign_key "posicao_fim_ano", "ativo"
  add_foreign_key "rendimento", "ativo"
end
