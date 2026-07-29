#!/bin/sh
set -eu

max_attempts=10
attempt=0

until mongosh \
  --username "$MONGO_INITDB_ROOT_USERNAME" \
  --password "$MONGO_INITDB_ROOT_PASSWORD" \
  --authenticationDatabase admin \
  --eval "db.adminCommand('ping')" >/dev/null 2>&1
do
  attempt=$((attempt + 1))

  if [ "$attempt" -ge "$max_attempts" ]; then
    echo "MongoDB did not become ready within ${max_attempts} seconds." >&2
    exit 1
  fi

  sleep 1
done

mongoimport \
  --username "$MONGO_INITDB_ROOT_USERNAME" \
  --password "$MONGO_INITDB_ROOT_PASSWORD" \
  --authenticationDatabase admin \
  --db hardwarenexus-products \
  --collection products \
  --file /scripts/products.json \
  --jsonArray \
  --mode=upsert \
  --upsertFields _id

echo "MongoDB seed complete"