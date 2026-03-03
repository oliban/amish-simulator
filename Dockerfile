# Unity WebGL build output is copied from Build/WebGL/
# Run: unity-editor -batchmode -buildTarget WebGL -projectPath . -buildPath Build/WebGL
# Then: docker build -t amish-simulator .

FROM nginx:1.27-alpine

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy our nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy Unity WebGL build output
# Unity outputs: index.html, Build/, TemplateData/, StreamingAssets/ (if any)
COPY Build/WebGL/ /usr/share/nginx/html/

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
