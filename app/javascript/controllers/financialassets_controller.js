import { Controller } from "@hotwired/stimulus"
import { FetchRequest } from '@rails/request.js'

export default class extends Controller {

  static targets = [
    "curr_avg_price",
    "ticker",
    "invested_amount",
    "calculated_average_price"]

  async load_curr_avg_price() {
    const request = new FetchRequest(
      'get',
      `http://localhost:3001/financial_assets/by_code/${this.tickerTarget.value}`
    )

    const response = await request.perform()

    if (response.ok) {
      const result = await response.json
      this.curr_avg_priceTarget.value = result.average_price
    }
  }

  async calculate_average_price() {

    const request = new FetchRequest(
      'get',
      `http://localhost:3001/financial_assets/${this.tickerTarget.value}/calculate_new_average_price/${this.invested_amountTarget.value}`
    )

    const response = await request.perform()

    if (response.ok) {
      const result = await response.json
      this.calculated_average_priceTarget.innerHTML = "Novo preço médio: " + result.new_average_price
    }
  }
}
