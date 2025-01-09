Rails.application.routes.draw do
  # Define your application routes per the DSL in https://guides.rubyonrails.org/routing.html

  # Reveal health status on /up that returns 200 if the app boots with no exceptions, otherwise 500.
  # Can be used by load balancers and uptime monitors to verify that the app is live.
  get "up" => "rails/health#show", as: :rails_health_check

  # Render dynamic PWA files from app/views/pwa/* (remember to link manifest in application.html.erb)
  # get "manifest" => "rails/pwa#manifest", as: :pwa_manifest
  # get "service-worker" => "rails/pwa#service_worker", as: :pwa_service_worker

  # Defines the root path route ("/")
  # root "posts#index"

  get 'irpf/', to: 'irpf#index'

  resources :reports, only: [] do
    collection do
      get 'resultados_mensais/:ano/:mes', action: 'resultados_mensais', as: :resultados_mensais
      get 'lucro-prejuizo', action: 'lucro_prejuizo', as: :lucro_prejuizo
    end
  end

  # Define your application routes per the DSL in https://guides.rubyonrails.org/routing.html

  # Defines the root path route ("/")
  # root "articles#index"
  resources :financial_assets, param: :codigo, only: [] do
    member do
      get 'calculate_new_average_price/:invested_amount', action: 'calculate_new_average_price'
    end
  end

  get '/financial_assets/by_codigo/:codigo', to: 'financial_assets#by_codigo'

  resources :events, only: [] do
    collection do
      post 'import_files'
    end
  end

  get 'import_files', to: 'files#import'
  post 'import_files', to: 'files#import_do'

  get 'new_average_price', to: 'financial_assets#new_average_price'
end
