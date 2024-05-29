#!/bin/bash
set -e

AWS_REGION="ap-southeast-1"

SECRET_NAME="env"

SECRET_VALUE=$(aws secretsmanager get-secret-value --region $AWS_REGION --secret-id $SECRET_NAME --query SecretString --output text)

echo "SECRET_VALUE: $SECRET_VALUE"

source ~/.bash_profile

# Save key-value pairs to .env file
echo "$SECRET_VALUE" | jq -r 'to_entries[] | "\(.key)=\(.value)"' > ~/.env

# Load environment variables from .env file
source ~/.env
