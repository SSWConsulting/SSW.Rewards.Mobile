#!/bin/bash
set -e  # Exit on error

echo "🚀 Docker Build, Start & Test Script"
echo "======================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to print colored messages
print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Step 1: Stop existing containers
print_info "Stopping existing containers..."
docker compose --profile all down || true
print_success "Containers stopped"
echo ""

# Step 2: Build containers
print_info "Building Docker containers (this may take a few minutes)..."
docker compose --profile all build --no-cache
print_success "Containers built successfully"
echo ""

# Step 3: Start containers
print_info "Starting all services..."
docker compose --profile all up -d
print_success "Services started"
echo ""

# Step 4: Wait for WebAPI to be ready (AdminUI depends on this)
print_info "Waiting for WebAPI to be ready..."
MAX_RETRIES=90
RETRY_COUNT=0
while ! curl -k -s https://localhost:5001/health > /dev/null 2>&1; do
    RETRY_COUNT=$((RETRY_COUNT+1))
    if [ $RETRY_COUNT -ge $MAX_RETRIES ]; then
        print_error "WebAPI failed to start after ${MAX_RETRIES} seconds"
        echo ""
        print_info "Container logs:"
        docker compose logs --tail=50 rewards-webapi
        exit 1
    fi
    echo -n "."
    sleep 1
done
echo ""

# Additional wait for WebAPI to fully initialize
print_info "Waiting additional 5 seconds for WebAPI to fully initialize..."
sleep 5
print_success "WebAPI is ready (https://localhost:5001/health)"
echo ""

# Step 5: Wait for AdminUI to be ready (now that WebAPI is running)
print_info "Waiting for AdminUI to be ready..."
RETRY_COUNT=0
MAX_RETRIES=90
while ! curl -k -s https://localhost:7137 > /dev/null; do
    RETRY_COUNT=$((RETRY_COUNT+1))
    if [ $RETRY_COUNT -ge $MAX_RETRIES ]; then
        print_error "AdminUI failed to start after ${MAX_RETRIES} seconds"
        echo ""
        print_info "Container logs:"
        docker compose logs --tail=50 rewards-adminui
        exit 1
    fi
    echo -n "."
    sleep 1
done
echo ""

# Additional wait for AdminUI to fully initialize
print_info "Waiting additional 10 seconds for AdminUI to fully initialize..."
sleep 10
print_success "AdminUI is ready (https://localhost:7137)"
echo ""

# Step 6: Run Playwright authentication
print_info "Running Playwright authentication setup..."
cd tools/ui-tests

# Verify .env file exists
if [ ! -f .env ]; then
    print_error ".env file not found in tools/ui-tests/"
    print_info "Please create .env with TEST_USER_EMAIL and TEST_USER_PASSWORD"
    exit 1
fi

if npx playwright test --project=setup --reporter=list; then
    print_success "Authentication successful"
else
    print_error "Authentication failed"
    echo ""
    print_info "Check if credentials in .env are correct"
    exit 1
fi
echo ""

# Step 7: Run Playwright tests
print_info "Running Playwright tests..."
echo ""
print_info "This will run all notification page tests (56 tests expected)"
echo ""

if npx playwright test tests/notifications/ --reporter=list; then
    print_success "All Playwright tests passed!"
    echo ""
    print_info "Test summary:"
    echo "  • Desktop tests: Page loads, table, search, pagination, etc."
    echo "  • Tablet tests: Touch interactions, landscape mode"
    echo "  • Mobile tests: Responsive layout, vertical scrolling"
    echo "  • SendNotification: Form, preview, validation"
else
    print_error "Some Playwright tests failed"
    echo ""
    print_info "Debugging steps:"
    echo "  1. Check browser console: npx playwright test --headed --debug"
    echo "  2. View test report: npx playwright show-report"
    echo "  3. Check service logs: docker compose logs rewards-adminui"
    echo "  4. Verify page loads: curl -k https://localhost:7137/notifications"
    echo ""
    print_info "Common issues:"
    echo "  • Page not loading: AdminUI still initializing (wait longer)"
    echo "  • Auth failed: Check credentials in .env"
    echo "  • Table not rendering: Check WebAPI connection"
    exit 1
fi
echo ""

# Step 8: Summary
echo "======================================"
print_success "🎉 All steps completed successfully!"
echo ""
echo "Services running:"
echo "  • AdminUI: https://localhost:7137"
echo "  • WebAPI:  https://localhost:5001"
echo "  • SQL:     localhost:1433"
echo "  • Azurite: localhost:10000-10002"
echo ""
echo "To stop services: docker compose --profile all down"
echo "To view logs:     docker compose logs -f [service-name]"
echo "======================================"
