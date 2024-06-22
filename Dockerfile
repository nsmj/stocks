FROM ruby:3.1.0-alpine AS build

RUN apk update &&\
    apk add nodejs npm build-base sqlite sqlite-dev tzdata bash git &&\
    apk add mupdf-tools gcc g++ &&\
    npm install --global yarn nodemon@2.0.20

RUN adduser -D dev
RUN mkdir /app && chown dev:dev /app

WORKDIR /app

COPY entrypoint.sh /usr/bin/
RUN chmod +x /usr/bin/entrypoint.sh
ENTRYPOINT ["entrypoint.sh"]
EXPOSE 3000

USER dev

COPY --chown=dev Gemfile /app/Gemfile
COPY --chown=dev Gemfile.lock /app/Gemfile.lock

RUN bundle install

COPY --chown=dev . .

CMD ["rails", "server", "-b", "0.0.0.0"]