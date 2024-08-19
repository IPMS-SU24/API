#!/bin/bash
AWS_REGION="ap-southeast-1"
AWS_ACCOUNT_ID="905418022082"
REPOSITORY_NAME="ipmg_api"

# Start Docker container 

# Tên của Docker container muốn khởi chạy
DOCKER_IMAGE="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:latest"

# Check container is exist
if [ "$(docker ps -a -q -f name=$REPOSITORY_NAME)" ]; then
    # If exist, delete docker container
    docker stop $REPOSITORY_NAME
    docker rm $REPOSITORY_NAME
fi

# Start Docker container
docker run -e TZ=Asia/Ho_Chi_Minh -d -e ASPNETCORE_URLS='http://+:443' --name $REPOSITORY_NAME -p 443:443 $DOCKER_IMAGE
docker image prune -f
# Check docker container status
if [ $? -eq 0 ]; then
  echo "Container đã được khởi chạy thành công với tên: $REPOSITORY_NAME và port 80 trên máy host đã được map với port 300 trên container."
else
  echo "Lỗi trong quá trình khởi chạy container."
  exit 1
fi

# End script
exit 0
