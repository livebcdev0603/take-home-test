#!/bin/bash

# API Test Script for Loan Management System
# This script tests all core API endpoints

API_URL="http://localhost:5000"

echo "=== Testing Loan Management API ==="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test 1: List all loans
echo "1. Testing GET /loans (List all loans)..."
RESPONSE=$(curl -s -w "\n%{http_code}" "$API_URL/loans")
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ Success${NC} (HTTP $HTTP_CODE)"
    echo "Response: $BODY" | head -c 200
    echo "..."
else
    echo -e "${RED}✗ Failed${NC} (HTTP $HTTP_CODE)"
fi
echo ""

# Test 2: Get loan by ID
echo "2. Testing GET /loans/1 (Get loan by ID)..."
RESPONSE=$(curl -s -w "\n%{http_code}" "$API_URL/loans/1")
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ Success${NC} (HTTP $HTTP_CODE)"
    echo "Response: $BODY"
else
    echo -e "${RED}✗ Failed${NC} (HTTP $HTTP_CODE)"
fi
echo ""

# Test 3: Create a new loan
echo "3. Testing POST /loans (Create loan)..."
RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/loans" \
  -H "Content-Type: application/json" \
  -d '{"amount": 10000, "applicantName": "Test User"}')
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')

if [ "$HTTP_CODE" -eq 201 ]; then
    echo -e "${GREEN}✓ Success${NC} (HTTP $HTTP_CODE)"
    LOAN_ID=$(echo "$BODY" | grep -o '"id":[0-9]*' | grep -o '[0-9]*' | head -1)
    echo "Created loan ID: $LOAN_ID"
    echo "Response: $BODY"
    export TEST_LOAN_ID=$LOAN_ID
else
    echo -e "${RED}✗ Failed${NC} (HTTP $HTTP_CODE)"
    echo "Response: $BODY"
fi
echo ""

# Test 4: Make a payment (if loan was created)
if [ ! -z "$TEST_LOAN_ID" ]; then
    echo "4. Testing POST /loans/$TEST_LOAN_ID/payment (Make payment)..."
    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/loans/$TEST_LOAN_ID/payment" \
      -H "Content-Type: application/json" \
      -d '{"amount": 2500}')
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | sed '$d')

    if [ "$HTTP_CODE" -eq 200 ]; then
        echo -e "${GREEN}✓ Success${NC} (HTTP $HTTP_CODE)"
        echo "Response: $BODY"
    else
        echo -e "${RED}✗ Failed${NC} (HTTP $HTTP_CODE)"
        echo "Response: $BODY"
    fi
    echo ""
else
    echo -e "${YELLOW}4. Skipping payment test (loan creation failed)${NC}"
    echo ""
fi

# Test 5: Error handling - Invalid payment amount
echo "5. Testing error handling (Invalid payment amount)..."
if [ ! -z "$TEST_LOAN_ID" ]; then
    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/loans/$TEST_LOAN_ID/payment" \
      -H "Content-Type: application/json" \
      -d '{"amount": -100}')
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | sed '$d')

    if [ "$HTTP_CODE" -eq 400 ]; then
        echo -e "${GREEN}✓ Success${NC} (HTTP $HTTP_CODE - Expected error)"
        echo "Response: $BODY"
    else
        echo -e "${RED}✗ Failed${NC} (Expected 400, got $HTTP_CODE)"
    fi
else
    echo -e "${YELLOW}Skipping (loan not created)${NC}"
fi
echo ""

# Test 6: Get non-existent loan
echo "6. Testing GET /loans/999 (Non-existent loan)..."
RESPONSE=$(curl -s -w "\n%{http_code}" "$API_URL/loans/999")
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)

if [ "$HTTP_CODE" -eq 404 ]; then
    echo -e "${GREEN}✓ Success${NC} (HTTP $HTTP_CODE - Expected 404)"
else
    echo -e "${RED}✗ Failed${NC} (Expected 404, got $HTTP_CODE)"
fi
echo ""

echo "=== Test Complete ==="

