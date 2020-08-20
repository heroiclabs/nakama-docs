Nakama Documentation
====================

> Documentation for Nakama social and realtime server.

This project uses Markdown for documentation which is compiled with [mkdocs](http://www.mkdocs.org).

## Run with Docker

### Development mode
```shell
docker-compose up dev
```
### Build files
```shell
docker-compose run build
```

## Install and setup
```shell
pip install pip --upgrade
pip install mkdocs==1.1.2
pip install mkdocs-material==5.5.6
```

*Note:* For Python 3.X, you may need to run the above commands using `pip3` instead of `pip`.

## Development

```
mkdocs serve
```
