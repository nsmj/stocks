class ProcessadorArquivosJson < ApplicationService
  def call
    Dir.glob('files/JsonFile/**/*').each do |file_path|
      arquivo_json = ArquivoJson.new

      arquivo_json.extrai_dados_arquivo(file_path).each do |j|
        raise StandardError, j.errors unless j.save
      end
    end
  end
end
