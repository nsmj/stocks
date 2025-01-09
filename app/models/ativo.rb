class Ativo < ApplicationRecord
  validates :codigo, presence: true, uniqueness: true
  validates :nome, presence: true
  validates :descricao_nota, presence: true
  validates :preco_medio, numericality: true, allow_nil: true
  validates :fator_preco_teto, numericality: true, allow_nil: true
  validates :preco_teto_medio, numericality: true, allow_nil: true
  validates :preco_teto_projetivo, numericality: true, allow_nil: true

  belongs_to :tipo_ativo, required: true
  has_many :operacoes, foreign_key: :ativo_id
  has_many :eventos, foreign_key: :ativo_id
  has_many :posicoes_fim_ano, foreign_key: :ativo_id

  def novo_preco_medio(montante_investido, preco)
    montante_investido = montante_investido.gsub(',', '.').to_f if montante_investido.is_a? String
    preco = preco.gsub(',', '.').to_f if preco.is_a? String

    quantidade = montante_investido / preco

    novo_preco = ((posicao * preco_medio) + montante_investido) / (quantidade + posicao)

    { 'preco_medio_antigo' => preco_medio,
      'preco_medio_novo' => novo_preco.round(2) }
  end

  # rails_admin do
  #   list do
  #     fields :id, :codigo, :nome, :cnpj, :cnpj_fonte_pagadora, :tipo_ativo,
  #            :preco_teto_medio, :preco_teto_projetivo
  #   end
  # end

  def to_param
    codigo
  end

  def preco
    conn = Faraday.new(
      url: 'https://brapi.dev',
      params: { token: ENV.fetch('BRAPI_DEV_TOKEN') }
    )

    response = conn.get("/api/quote/#{codigo}")
    JSON.parse(response.body)['results'].first['regularMarketPrice']
  end
end
