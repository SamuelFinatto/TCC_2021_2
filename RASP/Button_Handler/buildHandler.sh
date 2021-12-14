sudo apt-get install network-manager -y
sudo systemctl start NetworkManager.service 
sudo systemctl enable NetworkManager.service
docker build -t handler_tcc -f Dockerfile .