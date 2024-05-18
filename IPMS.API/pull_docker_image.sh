#!/bin/bash

# Thực hiện pull Docker image từ một registry cụ thể
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME
# Đường dẫn của Docker image cần pull
DOCKER_IMAGE="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:latest"

# Thực hiện pull Docker image
docker pull $DOCKER_IMAGE

# Kiểm tra xem quá trình pull có thành công không
if [ $? -eq 0 ]; then
  echo "Pull Docker image thành công: $DOCKER_IMAGE"
else
  echo "Lỗi trong quá trình pull Docker image: $DOCKER_IMAGE"
  exit 1
fi

# Kết thúc script
exit 0
