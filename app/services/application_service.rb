class ApplicationService
  def self.call(*args, **kargs, &block)
    new(*args, **kargs, &block).call
  end
end