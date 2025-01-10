require 'rails_helper'

RSpec.shared_examples 'verifica_operacao' do |indice_operacao, compra, quantidade, preco_ativo, valor_total, taxas|
  it 'deve extrair os dados da operação' do
    expect(operacoes[indice_operacao].compra).to eq(compra)
    expect(operacoes[indice_operacao].quantidade).to eq(quantidade)
    expect(operacoes[indice_operacao].preco_ativo).to eq(preco_ativo)
    expect(operacoes[indice_operacao].valor_total).to eq(valor_total)
    expect(operacoes[indice_operacao].taxas).to eq(taxas)
  end
end
