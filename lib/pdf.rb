# Handle PDF stuff.
module Pdf
  def self.extract_pdf_data(file_path)
    file_name = File.basename(file_path).gsub('.pdf', '.txt')

    system("mutool convert -p #{ENV.fetch('PDF_PASSWORD')} -F text -o txt/#{file_name} #{file_path} 2> /dev/null")

    File.readlines(File.join('txt', file_name)).map(&:strip)
  end
end
