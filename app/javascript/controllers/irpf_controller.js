import { Controller } from "@hotwired/stimulus"

export default class extends Controller {

  static targets = ["eyp_text"]

  copiar(event) {
    navigator.clipboard.writeText(
      event.currentTarget.innerHTML
    ).catch(console.error)
  }
}
