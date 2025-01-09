FROM ruby:3.3.0-slim AS build

RUN apt-get update && apt-get upgrade -y &&\
    apt-get install nodejs npm build-essential sqlite3 libsqlite3-dev tzdata bash git mupdf-tools gcc g++ -y &&\
    npm install --global yarn nodemon@2.0.20

RUN adduser --disabled-password dev
RUN mkdir /app && chown dev:dev /app

WORKDIR /app

#COPY entrypoint.sh /usr/bin/
#RUN chmod +x /usr/bin/entrypoint.sh
#ENTRYPOINT ["entrypoint.sh"]
EXPOSE 3000

USER dev

COPY --chown=dev Gemfile /app/Gemfile
COPY --chown=dev Gemfile.lock /app/Gemfile.lock

RUN bundle install

COPY --chown=dev . .

CMD ["rails", "server", "-b", "0.0.0.0"]