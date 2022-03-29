## Nakama documentation has moved.
> As part of our ongoing efforts to improve our developer experience, content and discoverability we've moved away from using MkDocs to host the content. Please read this blog post for more information:
> 
> 
>  https://heroiclabs.com/blog/announcements/announcing-docs-20

Nakama Documentation
====================

> Documentation for Nakama social and realtime server.

This project uses Markdown for documentation which is compiled with [mkdocs](http://www.mkdocs.org).

## Running

```
pip3 install -r requirements.txt
python3 -m mkdocs serve
```

or

### pipenv

```
pip3 install pipenv
pipenv install
pipenv run python3 -m mkdocs serve
```

### Docker

Run the `serve.sh` bash script.

```
chmod +x serve.sh
./serve.sh
```
