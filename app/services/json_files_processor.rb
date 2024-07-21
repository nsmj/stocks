class JsonFilesProcessor < ApplicationService

  def call
    Dir.glob('files/JsonFile/**/*').each do |file_path|
      json_file = JsonFile.new

      json_file.extract_file_data(file_path).each do |j|
        raise StandardError, j.errors unless j.save
      end
    end
  end
end