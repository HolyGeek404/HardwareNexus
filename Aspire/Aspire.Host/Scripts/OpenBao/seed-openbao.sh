#!/bin/sh
set -eu

: "${OPENBAO_USER_ROLE_ID:?OPENBAO_USER_ROLE_ID is required}"
: "${OPENBAO_USER_SECRET_ID:?OPENBAO_USER_SECRET_ID is required}"
: "${OPENBAO_PRODUCT_ROLE_ID:?OPENBAO_PRODUCT_ROLE_ID is required}"
: "${OPENBAO_PRODUCT_SECRET_ID:?OPENBAO_PRODUCT_SECRET_ID is required}"
: "${OPENBAO_CART_ROLE_ID:?OPENBAO_CART_ROLE_ID is required}"
: "${OPENBAO_CART_SECRET_ID:?OPENBAO_CART_SECRET_ID is required}"

export BAO_ADDR=http://127.0.0.1:8200
export BAO_TOKEN="${BAO_DEV_ROOT_TOKEN_ID:?BAO_DEV_ROOT_TOKEN_ID is required}"

ensure_secret_id() {
  role_name="$1"
  secret_id="$2"

  if bao write "auth/approle/role/${role_name}/secret-id/lookup" secret_id="$secret_id" >/dev/null 2>&1; then
    return
  fi

  bao write "auth/approle/role/${role_name}/custom-secret-id" secret_id="$secret_id" >/dev/null
}

until bao status >/dev/null 2>&1; do
  sleep 1
done

echo "OpenBao is up."

bao status


# --- Enable AppRole auth (ignore error if already enabled) ---
bao auth enable approle 2>/dev/null || echo "approle already enabled"

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

# --- Roles, with pinned RoleIDs so they stay stable across every reset ---

# --- User
bao write auth/approle/role/hardwarenexus-user \
  token_policies="hardwarenexus-user-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-user/role-id \
  role_id="$OPENBAO_USER_ROLE_ID"
ensure_secret_id hardwarenexus-user "$OPENBAO_USER_SECRET_ID"
# ---

# --- Product
bao write auth/approle/role/hardwarenexus-product \
  token_policies="hardwarenexus-product-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-product/role-id \
  role_id="$OPENBAO_PRODUCT_ROLE_ID"
ensure_secret_id hardwarenexus-product "$OPENBAO_PRODUCT_SECRET_ID"
# ---

# --- Cart
bao write auth/approle/role/hardwarenexus-cart \
  token_policies="hardwarenexus-cart-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-cart/role-id \
  role_id="$OPENBAO_CART_ROLE_ID"
ensure_secret_id hardwarenexus-cart "$OPENBAO_CART_SECRET_ID"
# ---

# --- Secrets (dev only) ---
bao kv put secret/hardwarenexus/api/product \
  mongodb-connstr="mongodb://dev_user:xdt60FPNOnxcDVdug75H3b9HFboWrNNBXSAiBgSS1rU=@localhost:27017/hardwarenexus-products?authSource=admin"

bao kv put secret/hardwarenexus/api/user \
  postgres-connstr="Host=localhost;Port=5432;Database=HardwareNexus;Username=dev_user;Password=XDJmD2ef/C5eW6cteabSWqGWgfeXv5q+MaCB5HLZH0U="

bao kv put secret/hardwarenexus/api/cart \
  redis-connstr="localhost:6379"

echo "OpenBao seed complete. AppRole credentials were registered."
