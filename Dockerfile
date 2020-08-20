FROM python:3.8.5

RUN pip3 install mkdocs mkdocs-material

EXPOSE 8000

COPY . /nakama-docs

WORKDIR /nakama-docs
