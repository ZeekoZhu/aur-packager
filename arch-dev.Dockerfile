FROM docker.io/archlinux:base-devel

RUN pacman -Syu --noconfirm --needed git sudo

RUN echo "container ALL=(ALL) NOPASSWD: ALL" >> /etc/sudoers

RUN useradd --create-home container

USER container

RUN git clone https://aur.archlinux.org/pikaur.git /tmp/pikaur
WORKDIR /tmp/pikaur

RUN makepkg -fsri --noconfirm

RUN rm -rf /tmp/pikaur
