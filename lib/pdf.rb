# Handle PDF stuff.
module Pdf
  def self.extract_pdf_data(file_path)
    file_name = File.basename(file_path).gsub('.pdf', '.txt')

    system(self.build_command(ENV.fetch('PDF_PASSWORD_1'), file_name, file_path))

    result = self.get_data(file_name)

    if result.empty?
      system(self.build_command(ENV.fetch('PDF_PASSWORD_2'), file_name, file_path))
      result = self.get_data(file_name)
    end

    result
  end

  private

  def self.build_command(pdf_password, file_name, file_path)
    "mutool convert -p #{pdf_password} -F text -o txt/#{file_name} #{file_path} 2> /dev/null"
  end

  def self.get_data(file_name)
    File.readlines(File.join('txt', file_name)).map(&:strip)
  end
end
