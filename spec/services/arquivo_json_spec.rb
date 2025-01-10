require 'rails_helper'
require './spec/support/verifica_operacao_spec'

RSpec.describe 'ArquivoJson' do
  let(:arquivo_json) { ArquivoJson.new }

  describe 'ExtrairDadosArquivo' do
    it 'deve extrair os eventos do arquivo json' do
      evento = arquivo_json.extrai_dados_arquivo('files_test/JsonFile/20200605.json').first

      expect(evento.data).to eq(Time.strptime('05/06/2020', '%d/%m/%Y'))
      expect(evento.fator).to eq(10)
      expect(evento.tipo_evento.nome).to eq('Grupamento')
      expect(evento.ativo.codigo).to eq('TCSA3')
    end

    it 'deve extrair as operações do arquivo json' do
      evento = arquivo_json.extrai_dados_arquivo('files_test/JsonFile/20200403.json').first

      expect(evento.data).to eq(Time.strptime('03/04/2020', '%d/%m/%Y'))
      expect(evento.quantidade).to eq(3)
      expect(evento.preco_ativo).to eq(2.97)
      expect(evento.compra).to eq(1)
      expect(evento.tipo_operacao.nome).to eq('Swing Trade')
      expect(evento.ativo.codigo).to eq('JHSF3')
      expect(evento.valor_total).to eq(8.91)
    end
  end
end
