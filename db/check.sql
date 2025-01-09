select
	sum(lucro_liquido),
	sum(quantidade),
	sum(quantidade),
	sum(preco_ativo),
	sum(valor_total),
	sum(taxas)
from operacao;

select
	sum(posicao),
	sum(preco_medio)
from ativo;

select tipo_ativo_id, count(*) from ativo
group by tipo_ativo_id;

select compra, count(*) from operacao
group by compra;

select ativo_id, count(*) from operacao
group by ativo_id;