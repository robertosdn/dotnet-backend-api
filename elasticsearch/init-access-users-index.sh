#!/bin/sh
set -eu

curl -fsS "http://elasticsearch:9200/_cluster/health" >/dev/null

curl -fsS -X PUT "http://elasticsearch:9200/access_users" \
  -H 'Content-Type: application/json' \
  -d '{
    "settings": {
      "number_of_shards": 1,
      "number_of_replicas": 0
    },
    "mappings": {
      "properties": {
        "id": {"type": "keyword"},
        "email": {"type": "keyword"},
        "name": {"type": "text"},
        "status": {"type": "keyword"},
        "version": {"type": "long"},
        "created_at": {"type": "date"},
        "updated_at": {"type": "date"}
      }
    }
  }' >/dev/null || echo "Index access_users already exists"

echo "Elasticsearch index access_users ready"
