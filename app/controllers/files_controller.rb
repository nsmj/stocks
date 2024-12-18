require 'pdf'
require 'zip'
require 'rake'

class FilesController < ApplicationController
  def import; end

  def import_do
    FileUnzipper.call(filter_params['file'])

    system('mkdir txt')

    Stocks::Application.load_tasks
    Rake::Task['db:reset'].invoke

    ProcessadorNotasCorretagem.call
    ProcessadorArquivosJson.call
    CalculadoraResultados.call

    FileUtils.rm_rf(Dir.glob('files/*'))

    redirect_to import_files_path
  end

  private

  def filter_params
    params.permit(:file)
  end
end
