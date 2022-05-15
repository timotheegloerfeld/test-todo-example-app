from docker.io/library/postgres:14

ENV POSTGRES_PASSWORD=mysecretpassword

EXPOSE 5432

COPY init.sql /docker-entrypoint-initdb.d/