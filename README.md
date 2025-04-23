# 📦 MS Notificação

micro serviço gestor de notificações

## Objetivos

Este repositório contém o microserviço de notificação, desenvolvido utilizando .NET 8 e o disparo de e-mail é feito via SendGrid. 
O processo de build, publicação e deployment funciona via workflow no GitHub Actions.

## Diagrama da Arquitetura de Infraestrutura
![Diagrama da Arquitetura de Infraestrutura](/FastVideo.drawio.png)

## SendGrid
é uma plataforma de comunicação com o cliente baseada em Denver, Colorado, para e-mail transacional e de marketing. Foi criada uma API Key no SendGrid com o e-mail de um dos autores do projeto. Assim, os e-mails disparados via requisição SQS serão enviados por este e-mail.

## Autores
### Fiap turma 8SOAT - Grupo 7

- André Bessa - RM357159
- Fernanda Beato - RM357346
- Felipe Bergmann - RM357042
- Darlei Randel - RM356751
- Victor Oliver - RM357451
