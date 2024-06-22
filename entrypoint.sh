#!/bin/bash
set -e

# Remove a potentially pre-existing server.pid for Rails.
rm -f /app/tmp/pids/server.pid

#bundle exec rake db:migrate 2>/dev/null || bundle exec rake db:setup
#bundle exec rails db:reset
#RAILS_ENV=test bundle exec rails db:reset

# Then exec the container's main process (what's set as CMD in the Dockerfile).
exec "$@"
