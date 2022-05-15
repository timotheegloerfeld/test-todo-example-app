#!/bin/sh

podman build -t todo-example-db .
podman run -d -p 5432:5432 --name todo-example-db todo-example-db