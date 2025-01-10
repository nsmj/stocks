def testa_operacao(operacao, compra, quantidade, preco_ativo, valor_total, taxas)
  expect(operacao.compra).to eq(compra)
  expect(operacao.quantidade).to eq(quantidade)
  expect(operacao.preco_ativo).to eq(preco_ativo)
  expect(operacao.valor_total).to eq(valor_total)
  expect(operacao.taxas).to eq(taxas)
end
