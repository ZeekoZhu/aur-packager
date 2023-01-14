FROM zeekozhu/arch-dev:latest

ENV ASPNETCORE_URLS=http://+:5000
WORKDIR /home/container

COPY --chown=container:container ./publish/apps/web/. ./apps/web/

WORKDIR /home/container/apps/web
ENTRYPOINT [ "/home/container/apps/web/AurPackager.Web" ]
