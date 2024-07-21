class BrokerNotesProcessor < ApplicationService

  def call
    Dir.glob('files/BrokerNotes/**/*').each do |file_path|
      next if File.directory? file_path

      broker = BrokerNote.get_broker(file_path)

      broker_note = BrokerNote.new(broker)

      broker_note.extract_file_data(file_path)

      broker_note.trades.each do |t|
        raise StandardError, t.errors unless t.save
      end

      broker_note.irrfs.each do |i|
        raise StandardError, i.errors unless i.save
      end
    end
  end
end