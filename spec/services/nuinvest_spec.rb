require 'rails_helper'
require './spec/support/verifica_operacao_spec'

RSpec.describe 'Nuinvest' do
  let(:nota_corretagem) { NotaCorretagem.new(Nuinvest.new) }

  describe 'ExtrairDadosArquivo' do
    it 'deve extrair a data da nota de corretagem' do
      nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Nuinvest/20191104_Invoice_19267.pdf')
      expect(nota_corretagem.data).to eq(Time.strptime('04/11/2019', '%d/%m/%Y'))
    end


    it 'deve extrair os dados das operações' do
      nota_corretagem = NotaCorretagem.new(Nuinvest.new)
      nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Nuinvest/20191104_Invoice_19267.pdf')

      operacoes = nota_corretagem.operacoes

      testa_operacao(operacoes[0], 1, 76, 13.84, 1051.84, 0.32)
      testa_operacao(operacoes[1], 1, 1, 13.82, 13.82, 0.00)
      testa_operacao(operacoes[2], 0, 6, 92.54, 555.24, 0.17)
      testa_operacao(operacoes[3], 0, 5, 103.07, 515.35, 0.16)
    end
  end
end
