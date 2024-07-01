module ApplicationHelper
  def to_currency(value)
    number_to_currency(value, locale: :"pt-BR", negative_format: "%u -%n")
  end
end
