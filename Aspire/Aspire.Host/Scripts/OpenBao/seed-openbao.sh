#!/bin/sh
set -eu
max_attempts=10
attempt=0

: "${OPENBAO_USER_ROLE_ID:?OPENBAO_USER_ROLE_ID is required}"
: "${OPENBAO_USER_SECRET_ID:?OPENBAO_USER_SECRET_ID is required}"
: "${OPENBAO_PRODUCT_ROLE_ID:?OPENBAO_PRODUCT_ROLE_ID is required}"
: "${OPENBAO_PRODUCT_SECRET_ID:?OPENBAO_PRODUCT_SECRET_ID is required}"
: "${OPENBAO_CART_ROLE_ID:?OPENBAO_CART_ROLE_ID is required}"
: "${OPENBAO_CART_SECRET_ID:?OPENBAO_CART_SECRET_ID is required}"

export BAO_ADDR=http://127.0.0.1:8200
export BAO_TOKEN="${BAO_DEV_ROOT_TOKEN_ID:?BAO_DEV_ROOT_TOKEN_ID is required}"

until bao status >/dev/null 2>&1; do
    attempt=$((attempt + 1))
  
    if [ "$attempt" -ge "$max_attempts" ]; then
      echo "OpenBao did not become ready within ${max_attempts} seconds." >&2
      exit 1
    fi
  
    sleep 1
done

echo "OpenBao is up."

bao status


# --- Enable AppRole auth (ignore error if already enabled) ---
if ! bao auth list -format=json | grep -q '"approle/"'; then
  bao auth enable approle
fi
# --- Policies ---
bao policy write hardwarenexus-user-policy - <<EOF
path "secret/data/hardwarenexus/api/user" {
  capabilities = ["read"]
}
EOF

bao policy write hardwarenexus-product-policy - <<EOF
path "secret/data/hardwarenexus/api/product" {
  capabilities = ["read"]
}
EOF

bao policy write hardwarenexus-cart-policy - <<EOF
path "secret/data/hardwarenexus/api/cart" {
  capabilities = ["read"]
}
EOF

# --- User
bao write auth/approle/role/hardwarenexus-user \
  token_policies="hardwarenexus-user-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-user/role-id \
  role_id="$OPENBAO_USER_ROLE_ID"
  
bao write auth/approle/role/hardwarenexus-user/custom-secret-id \
  secret_id="$OPENBAO_USER_SECRET_ID"
# ---

# --- Product
bao write auth/approle/role/hardwarenexus-product \
  token_policies="hardwarenexus-product-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-product/role-id \
  role_id="$OPENBAO_PRODUCT_ROLE_ID"
  
bao write auth/approle/role/hardwarenexus-product/custom-secret-id \
  secret_id="$OPENBAO_PRODUCT_SECRET_ID"
# ---

# --- Cart
bao write auth/approle/role/hardwarenexus-cart \
  token_policies="hardwarenexus-cart-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-cart/role-id \
  role_id="$OPENBAO_CART_ROLE_ID"
  
bao write auth/approle/role/hardwarenexus-cart/custom-secret-id \
  secret_id="$OPENBAO_CART_SECRET_ID"
# ---

# --- Secrets (dev only) ---
bao kv put secret/hardwarenexus/api/product \
  mongodb-connstr="mongodb://dev_user:xdt60FPNOnxcDVdug75H3b9HFboWrNNBXSAiBgSS1rU=@localhost:27017/hardwarenexus-products?authSource=admin"

bao kv put secret/hardwarenexus/api/user \
  postgres-connstr="Host=localhost;Port=5432;Database=HardwareNexus;Username=dev_user;Password=XDJmD2ef/C5eW6cteabSWqGWgfeXv5q+MaCB5HLZH0U="

bao kv put secret/hardwarenexus/api/cart \
  redis-connstr="localhost:6379"

echo "OpenBao seed complete. AppRole credentials were registered."
