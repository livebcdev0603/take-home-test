#!/bin/bash

# Health Check Script for Loan Management System

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=== Health Check ==="
echo ""

# Check backend API
echo "1. Checking backend API..."
if curl -s -f http://localhost:5000/loans > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Backend API is running on http://localhost:5000${NC}"
    
    # Check if we get valid JSON
    LOAN_COUNT=$(curl -s http://localhost:5000/loans | grep -o '"id"' | wc -l)
    echo "   Found $LOAN_COUNT loan(s) in database"
else
    echo -e "${RED}✗ Backend API is not accessible on http://localhost:5000${NC}"
    echo "   Please start the backend: cd backend/src && docker-compose up -d"
fi
echo ""

# Check frontend
echo "2. Checking frontend..."
if curl -s -f http://localhost:4200 > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Frontend is running on http://localhost:4200${NC}"
else
    echo -e "${YELLOW}⚠ Frontend is not accessible on http://localhost:4200${NC}"
    echo "   Start with: cd frontend && npm start"
fi
echo ""

# Check Docker containers
echo "3. Checking Docker containers..."
if command -v docker &> /dev/null; then
    if docker ps | grep -q loanmanagement; then
        echo -e "${GREEN}✓ Docker containers are running:${NC}"
        docker ps | grep loanmanagement | awk '{print "   - " $1 " (" $2 ")"}'
    else
        echo -e "${YELLOW}⚠ No loanmanagement containers found${NC}"
        echo "   Start with: cd backend/src && docker-compose up -d"
    fi
else
    echo -e "${YELLOW}⚠ Docker is not installed${NC}"
fi
echo ""

# Check SQL Server connection
echo "4. Checking SQL Server connection..."
if docker ps | grep -q sqlserver; then
    echo -e "${GREEN}✓ SQL Server container is running${NC}"
else
    echo -e "${YELLOW}⚠ SQL Server container is not running${NC}"
fi
echo ""

# Check if Node.js is available
echo "5. Checking Node.js..."
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    echo -e "${GREEN}✓ Node.js is installed (version $NODE_VERSION)${NC}"
else
    echo -e "${YELLOW}⚠ Node.js is not installed${NC}"
fi
echo ""

echo "=== Health Check Complete ==="
echo ""
echo "Next steps:"
echo "  - If backend is down: cd backend/src && docker-compose up -d"
echo "  - If frontend is down: cd frontend && npm start"
echo "  - Run API tests: ./test-api.sh"
echo "  - Run backend tests: cd backend/src && dotnet test"

