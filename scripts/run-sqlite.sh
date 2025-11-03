#!/bin/bash

# Mumii Microservices - SQLite Quick Start Script
# Chạy tất cả services với SQLite database

echo "🚀 Starting Mumii Microservices with SQLite..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to check if port is in use
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null ; then
        echo -e "${RED}❌ Port $1 is already in use${NC}"
        return 1
    else
        echo -e "${GREEN}✅ Port $1 is available${NC}"
        return 0
    fi
}

# Function to start service
start_service() {
    local service_name=$1
    local port=$2
    local path=$3
    
    echo -e "${BLUE}🔄 Starting $service_name on port $port...${NC}"
    
    if check_port $port; then
        cd "$path"
        dotnet run --urls "http://localhost:$port" &
        echo -e "${GREEN}✅ $service_name started successfully${NC}"
        sleep 2
    else
        echo -e "${RED}❌ Failed to start $service_name${NC}"
        return 1
    fi
}

# Check if we're in the right directory
if [ ! -f "Mumii.Microservices.sln" ]; then
    echo -e "${RED}❌ Please run this script from the project root directory${NC}"
    exit 1
fi

# Restore packages
echo -e "${YELLOW}📦 Restoring packages...${NC}"
dotnet restore

# Start services
echo -e "${YELLOW}🚀 Starting services...${NC}"

# Auth Service
start_service "Auth Service" 8081 "src/Services/Auth/Mumii.Auth.Api"

# Discovery Service  
start_service "Discovery Service" 8082 "src/Services/Discovery/Mumii.Discovery.Api"

# Social Service
start_service "Social Service" 8083 "src/Services/Social/Mumii.Social.Api"

# AI Service
start_service "AI Service" 8084 "src/Services/AI/Mumii.AI.Api"

# API Gateway
start_service "API Gateway" 8080 "src/ApiGateway"

# Wait a moment for all services to start
echo -e "${YELLOW}⏳ Waiting for services to initialize...${NC}"
sleep 5

# Check service health
echo -e "${BLUE}🔍 Checking service health...${NC}"

services=("8080:API Gateway" "8081:Auth Service" "8082:Discovery Service" "8083:Social Service" "8084:AI Service")

for service in "${services[@]}"; do
    port=$(echo $service | cut -d: -f1)
    name=$(echo $service | cut -d: -f2-)
    
    if curl -s "http://localhost:$port/health" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ $name is healthy${NC}"
    else
        echo -e "${RED}❌ $name is not responding${NC}"
    fi
done

echo ""
echo -e "${GREEN}🎉 All services started!${NC}"
echo ""
echo -e "${BLUE}📋 Service URLs:${NC}"
echo -e "  🌐 API Gateway:    http://localhost:8080"
echo -e "  🔐 Auth Service:   http://localhost:8081"
echo -e "  🏪 Discovery:      http://localhost:8082"
echo -e "  📝 Social Service: http://localhost:8083"
echo -e "  🤖 AI Service:     http://localhost:8084"
echo ""
echo -e "${BLUE}📚 Swagger UI:${NC}"
echo -e "  🎯 Centralized:    http://localhost:8080"
echo ""
echo -e "${YELLOW}💡 Database Files:${NC}"
echo -e "  📁 auth.db      - Auth Service database"
echo -e "  📁 discovery.db - Discovery Service database"
echo -e "  📁 social.db    - Social Service database"
echo ""
echo -e "${BLUE}🛑 To stop all services:${NC}"
echo -e "  Press Ctrl+C or run: ./scripts/stop-sqlite.sh"
echo ""

# Keep script running
echo -e "${YELLOW}⏳ Services are running... Press Ctrl+C to stop${NC}"
wait
