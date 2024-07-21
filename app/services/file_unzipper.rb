class FileUnzipper < ApplicationService

  def initialize(file)
    @file = file
  end

  def call
    File.binwrite(@file.original_filename, @file.read)

    Zip::File.open(@file.original_filename) do |zip_file|
      zip_file.each do |f|
        f_path = File.join('files', f.name)
        FileUtils.mkdir_p(File.dirname(f_path))
        zip_file.extract(f, f_path) unless File.exist?(f_path)
      end
    end

    File.delete(@file.original_filename)
  end
end