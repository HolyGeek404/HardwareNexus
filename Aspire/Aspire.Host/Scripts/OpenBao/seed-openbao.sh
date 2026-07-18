
export BAO_ADDR=http://127.0.0.1:8200
export BAO_TOKEN="9AsG2eYd7TseuOj3FVUo7yXWifSTuh2ZMJx67egZjsU="

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
  role_id="hardwarenexus-user-role-id"
# ---

# --- Product
bao write auth/approle/role/hardwarenexus-product \
  token_policies="hardwarenexus-product-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-product/role-id \
  role_id="hardwarenexus-product-role-id"
# ---

# --- User
bao write auth/approle/role/hardwarenexus-cart \
  token_policies="hardwarenexus-cart-policy" \
  token_ttl=1h \
  token_max_ttl=4h

bao write auth/approle/role/hardwarenexus-cart/role-id \
  role_id="hardwarenexus-cart-role-id"
# ---

# --- Secrets (dev only) ---
bao kv put secret/hardwarenexus/api/product \
  mongodb-connstr="mongodb://dev_user:xdt60FPNOnxcDVdug75H3b9HFboWrNNBXSAiBgSS1rU=@localhost:27017/hardwarenexus-products?authSource=admin"

bao kv put secret/hardwarenexus/api/user \
  postgres-connstr="Host=localhost;Port=5432;Database=hardwarenexus-core;Username=postgres;Password=supersecret"

bao kv put secret/hardwarenexus/api/cart \
  redis-connstr="localhost:6379"

# --- Generate fresh SecretIDs and write them to .env.local for Aspire to read ---
USER_SECRET_ID=$(bao write -f -field=secret_id auth/approle/role/hardwarenexus-user/secret-id)
PRODUCT_SECRET_ID=$(bao write -f -field=secret_id auth/approle/role/hardwarenexus-product/secret-id)
CART_SECRET_ID=$(bao write -f -field=secret_id auth/approle/role/hardwarenexus-cart/secret-id)

cat > /output/.env.local <<EOF
OPENBAO_USER_ROLE_ID=hardwarenexus-user-role-id
OPENBAO_USER_SECRET_ID=${USER_SECRET_ID}

OPENBAO_PRODUCT_ROLE_ID=hardwarenexus-product-role-id
OPENBAO_PRODUCT_SECRET_ID=${PRODUCT_SECRET_ID}

OPENBAO_CART_ROLE_ID=hardwarenexus-cart-role-id
OPENBAO_CART_SECRET_ID=${CART_SECRET_ID}
EOF

if [ ! -f /output/.env.local ]; then
  echo "ERROR: failed to write .env.local — check that /out is mounted correctly"
  exit 1
fi

echo ""
echo "Seed complete. Wrote fresh SecretIDs to .env.local"