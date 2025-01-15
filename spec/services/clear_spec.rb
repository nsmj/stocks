require 'rails_helper'
require './spec/support/verifica_operacao_spec'

RSpec.describe 'Clear' do
  let(:nota_corretagem) { NotaCorretagem.new(Clear.new) }

  describe 'ExtrairDadosArquivo' do
    it 'deve extrair a data da nota de corretagem' do
      nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Clear/20201202.pdf')
      expect(nota_corretagem.data).to eq(Time.strptime('02/12/2020', '%d/%m/%Y'))
    end

    it 'deve extrair os dados das operações comuns' do
      nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Clear/20201202.pdf')

      operacao = nota_corretagem.operacoes.first

      testa_operacao(operacao, 1, 14, 14.53, 203.42, 0.06)
    end

    it 'deve extrair os dados do Day Trade' do
      nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Clear/20210715.pdf')

      operacao = nota_corretagem.operacoes.first

      testa_operacao(operacao, 1, 100, 31.68, 3168, 0.73)
    end

    context 'Extracao de IRRF' do
      let(:irrf_teste) { nota_corretagem.irrfs.first }

      it 'deve extrair os dados do IRRF das operações comuns' do
        nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Clear/20210706.pdf')

        expect(irrf_teste.valor).to eq(0.23)
        expect(irrf_teste.data).to eq(Time.strptime('06/07/2021', '%d/%m/%Y'))
        expect(irrf_teste.tipo_operacao.nome).to eq('Swing Trade')
      end

      it 'deve extrair os dados do IRRF dos FIIs' do
        nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Clear/20210201.pdf')

        expect(irrf_teste.valor).to eq(0.07)
        expect(irrf_teste.tipo_operacao.nome).to eq('FII')
      end

      it 'deve extrair os dados do IRRF dos Day Trade' do
        nota_corretagem.extrai_dados_arquivo('files_test/BrokerNotes/Clear/20210728.pdf')

        expect(irrf_teste.valor).to eq(1.26)
        expect(irrf_teste.tipo_operacao.nome).to eq('Day Trade')
      end
    end
  end
end
