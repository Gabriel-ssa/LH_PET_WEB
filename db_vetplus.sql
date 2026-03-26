USE db_vetplus;

CREATE TABLE tb_usuario(
pk_usuario INT auto_increment primary key,
nm_usuario varchar(255) not null default 'Usuário do Sistema',
nm_email varchar(150) not null unique,
ds_senha_hash varchar(255) not null,
ft_senha_temporaria boolean not null default false,
ds_perfil varchar(50) not null,
fl_ativo boolean not null default true
);

create table tb_cliente(
pk_cliente int auto_increment primary key,
fk_usuario int not null,
nm_cliente varchar(255) not null,
cd_cpf varchar(14) not null unique,
cd_telefone varchar(20) not null,
foreign key (fk_usuario) references tb_usuario(pk_usuario) on delete cascade
);

create table tb_pet(
pk_pet int auto_increment primary key,
fk_cliente int not null,
nm_pet varchar(255) not null,
ds_especie varchar(100) not null,
ds_raca varchar(100) not null,
dt_nascimento date not null	default '2023-01-01',
foreign key (fk_cliente) references tb_cliente(pk_cliente) on delete cascade
);

create table tb_fornecedor(
pk_fornecedor int auto_increment primary key,
cd_cnpj varchar(18) not null unique,
nm_fornecedor varchar(255) not null,
nm_email varchar(150) not null
);

create table tb_produto(
pk_produto int auto_increment primary key,
nm_produto varchar(255) not null,
vl_preco decimal(10,2) not null,
vl_estoque int not null default 0,
ds_imagem_url text
);

create table tb_agendamento(
pk_agendamento int auto_increment primary key,
fk_pet int not null,
dt_data_hora datetime not null,
ds_tipo varchar(100) not null,
ds_status varchar(50) not null,
foreign key (fk_pet) references tb_pet(pk_pet) on delete cascade
);

create table tb_atendimento(
pk_atendimento int auto_increment primary key,
fk_agendamento int not null unique,
ds_prontuario text not null,
ds_diagnostico text not null,
ds_prescricao text not null,
foreign key (fk_agendamento) references tb_agendamento(pk_agendamento) on delete cascade
);

create table db_vetplus.tb_configuracao_clinica(
pk_configuracao int auto_increment primary key,
tm_abertura time not null,
tm_fechamento time not null,
ds_dias_trabalho varchar(50) not null,
vl_minutos_consulta int not null,
vl_minutos_banho int not null,
vl_minutos_tosa int not null
);

insert into db_vetplus.tb_configuracao_clinica(pk_configuracao, tm_abertura, tm_fechamento, ds_dias_trabalho, vl_minutos_consulta, vl_minutos_banho, vl_minutos_tosa)
values (1, '08:00:00', '18:00:00', '1,2,3,4,5,6', 30, 60,60);

create table dt_vetplus.tb_venda(
pk_venda int auto_increment primary key,
dt_venda datetime not null,
vl_total decimal(10,2) not null,
ds_forma_pagamento varchar(50) not null,
fk_usuario int not null,
foreign key (fk_usuario) references db_vetplus.tb_usuario(pk_usuario)
);

create table db_vetplus.tb_item_venda(
pk_item_venda int auto_increment primary key, 
fk_venda int not null,
fk_produto int not null,
vl_quantidade int not null,
vl_preco_unitario decimal(10,2) not null,
foreign key (fk_venda) references db_vetplus.tb_venda(pk_venda) on delete cascade,
foreign key (fk_produto) references db_vetplus.tb_produto(pk_produto)
);