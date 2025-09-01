
﻿# HeliCarry

Solução completa para simulação e gerenciamento de entregas com drones, composta por uma API robusta em ASP.NET Core (.NET 8) e uma interface web moderna em React + TypeScript + Vite.

---

## Visão Geral

O sistema permite cadastrar drones, criar pedidos, simular entregas, visualizar o estado da cidade em tempo real e acompanhar estatísticas detalhadas. A arquitetura é dividida em backend (API RESTful) e frontend (SPA React), com comunicação via HTTP.

---

## Funcionalidades

### Backend (.NET 8, C# 12)

- **Gerenciamento de Drones:** Cadastro, consulta, atualização e remoção de drones, com validação de capacidade, autonomia e status.
- **Pedidos de Entrega:** Criação, listagem e atualização de pedidos, incluindo informações de localização, peso, prioridade e status.
- **Simulação de Entregas:** Associação automática de pedidos a drones disponíveis, cálculo de rotas (A*), planejamento de entregas (Greedy), e atualização de status em tempo real.
- **Mapa da Cidade:** Modelagem de nós (bases, obstáculos, clientes) e geração de grafo para simulação de trajetos.
- **Serviços de Domínio:** Serviços para pathfinding (AStarService), cálculo de distâncias, planejamento de entregas e atribuição automática de pedidos.
- **API RESTful:** Endpoints para drones, pedidos, entregas e estado da cidade, com serialização JSON e tratamento de ciclos de referência.
- **Swagger:** Documentação automática dos endpoints para facilitar testes e integração.
- **Banco de Dados:** Persistência via Entity Framework Core e MySQL.

### Frontend (React + TypeScript + Vite)

- **Dashboard:** Exibe estatísticas gerais, como quantidade de entregas, tempo médio e drone mais eficiente.
- **CityMap:** Visualização gráfica da cidade em uma grade 100x100, mostrando bases, obstáculos, drones e pedidos em tempo real.
- **Formulários de Cadastro:** Componentes para criação de drones (`DroneForm`), pedidos (`PedidosForm`) e entregas (`EntregaForm`).
- **Listagens e Simulação:** Componentes para listar pedidos, entregas e simular operações de entrega.
- **Integração com API:** Consumo dos endpoints REST usando Axios, com atualização automática dos dados.
- **Tipagem Forte:** Uso de TypeScript para garantir segurança e clareza na manipulação dos dados.

---

## Estrutura dos Principais Arquivos

### Backend

- `Program.cs`: Configuração dos serviços, DI, middlewares, inicialização do banco e endpoints.
- `Modelos/`: Modelos de domínio (`Pedido`, `Entrega`, `Drone`, `Cidade`, etc.).
- `Serviços/`: Serviços de lógica de negócio (grafo, pathfinding, planejamento, atribuição automática).
- `Controllers/`: Endpoints REST para drones, pedidos, entregas e cidade.
- `AppDbContext.cs`: Contexto do Entity Framework Core.

### Frontend

- `src/components/CityMap.tsx`: Renderiza o mapa da cidade e estatísticas em tempo real.
- `src/components/DroneForm.tsx`: Formulário para cadastro de drones.
- `src/components/Dashboard.tsx`: Painel de estatísticas gerais.
- `src/components/EntregaForm.tsx`, `src/components/EntregaSimulacao.tsx`, `src/components/PedidosForm.tsx`: Formulários e simulação de entregas e pedidos.
- `src/services/api.ts`: Configuração do Axios para comunicação com a API.
- `src/types/simulacao.ts`: Tipos TypeScript para entidades do domínio.

---

## Como Executar

### Backend

1. Configure a string de conexão no `appsettings.json`.
2. Execute as migrações do banco de dados, se necessário.
3. Inicie o servidor com `dotnet run` ou pelo Visual Studio.
4. Acesse o Swagger em `/swagger` para explorar a API.

### Frontend

1. Instale as dependências com `npm install`.
2. Inicie o frontend com `npm run dev`.
3. Acesse a interface web para gerenciar drones, pedidos e acompanhar as entregas no mapa.

---

## Tecnologias Utilizadas

- **Backend:** ASP.NET Core (.NET 8), Entity Framework Core, MySQL
- **Frontend:** React, TypeScript, Vite, Axios, Bootstrap
- **Outros:** Swagger, ESLint, Serviços de pathfinding e planejamento customizados

---

## Observações

- O sistema é extensível para simulações mais complexas, integração com mapas reais e regras de negócio adicionais.
- O frontend utiliza tipagem forte para integração com a API e atualização reativa dos dados.
- O backend implementa lógica de atribuição automática de pedidos e simulação de rotas otimizadas para drones.

---
