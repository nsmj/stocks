class JsonFile
  def extract_file_data(file_path)
    json_content = JSON.parse(File.read(file_path))

    result = []

    json_content['trades']&.each do |t|
      result << Trade.from_json(t)
    end

    json_content['events']&.each do |e|
      result << Event.from_json(e)
    end

    result
  end
end
