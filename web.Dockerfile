FROM docker.io/archlinux:base-devel

RUN pacman -Syu --noconfirm --needed git sudo

RUN echo "container ALL=(ALL) NOPASSWD: ALL" >> /etc/sudoers

RUN useradd --create-home container

USER container

RUN git clone https://aur.archlinux.org/paru-git.git /tmp/paru
WORKDIR /tmp/paru

RUN makepkg -si --noconfirm

RUN rm -rf /tmp/paru


ENV ASPNETCORE_URLS=http://+:5000
WORKDIR /home/container

COPY --chown=container:container ./publish/apps/web/. ./apps/web/

WORKDIR /home/container/apps/web
ENTRYPOINT [ "/home/container/apps/web/AurPackager.Web" ]
