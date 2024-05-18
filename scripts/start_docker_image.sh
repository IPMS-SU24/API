#!/bin/bash
AWS_REGION="ap-southeast-1"
AWS_ACCOUNT_ID="905418022082"
REPOSITORY_NAME="ipmg_api"

# Thực hiện start Docker container từ Docker image đã được pull và map cổng 80 trên máy host với cổng 300 trên container

# Tên của Docker container muốn khởi chạy
DOCKER_IMAGE="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:latest"

# Kiểm tra xem container đã tồn tại hay không
if [ "$(docker ps -a -q -f name=$REPOSITORY_NAME)" ]; then
    # Nếu container tồn tại, thì stop và xóa container cũ
    docker stop $REPOSITORY_NAME
    docker rm $REPOSITORY_NAME
fi

# Thực hiện start Docker container và map cổng 80 trên máy host với cổng 300 trên container
docker run -d --name $REPOSITORY_NAME -p 80:80 $DOCKER_IMAGE

# Kiểm tra xem container đã khởi chạy thành công hay không
if [ $? -eq 0 ]; then
  echo "Container đã được khởi chạy thành công với tên: $REPOSITORY_NAME và port 80 trên máy host đã được map với port 300 trên container."
else
  echo "Lỗi trong quá trình khởi chạy container."
  exit 1
fi

# Kết thúc script
exit 0