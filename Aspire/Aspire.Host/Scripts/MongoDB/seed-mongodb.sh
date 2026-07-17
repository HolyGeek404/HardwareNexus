#!/bin/sh
set -e

echo "Waiting for MongoDB..."

until mongosh --eval "db.adminCommand('ping')" >/dev/null 2>&1; do
    sleep 1
done

echo "MongoDB is ready"

mongoimport \
    --db hardwarenexus-products \
    --collection products \
    --file /scripts/products.json \
    --jsonArray

echo "MongoDB seed complete"