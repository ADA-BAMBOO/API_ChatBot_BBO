PGDMP          	            }         	   ChatBotAI    17.2    17.2 I    ~           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                           false                       0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                           false            �           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                           false            �           1262    16388 	   ChatBotAI    DATABASE     �   CREATE DATABASE "ChatBotAI" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'Vietnamese_Vietnam.1252';
    DROP DATABASE "ChatBotAI";
                     postgres    false            �            1259    16399    bbo_chathistory    TABLE       CREATE TABLE public.bbo_chathistory (
    chatid integer NOT NULL,
    userid integer,
    message character varying(1000),
    response text,
    sentat timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    language_code character varying(10),
    responsetime numeric(10,2)
);
 #   DROP TABLE public.bbo_chathistory;
       public         heap r       postgres    false            �            1259    16409    bbo_chathistory_bk    TABLE       CREATE TABLE public.bbo_chathistory_bk (
    chatid integer NOT NULL,
    userid integer,
    message character varying(1000),
    response text,
    sentat timestamp without time zone,
    language_code character varying(10),
    responsetime numeric(10,2)
);
 &   DROP TABLE public.bbo_chathistory_bk;
       public         heap r       postgres    false            �            1259    16408    bbo_chathistory_bk_chatid_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_chathistory_bk_chatid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public.bbo_chathistory_bk_chatid_seq;
       public               postgres    false    222            �           0    0    bbo_chathistory_bk_chatid_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE public.bbo_chathistory_bk_chatid_seq OWNED BY public.bbo_chathistory_bk.chatid;
          public               postgres    false    221            �            1259    16398    bbo_chathistory_chatid_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_chathistory_chatid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public.bbo_chathistory_chatid_seq;
       public               postgres    false    220            �           0    0    bbo_chathistory_chatid_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE public.bbo_chathistory_chatid_seq OWNED BY public.bbo_chathistory.chatid;
          public               postgres    false    219            �            1259    16480 
   bbo_credit    TABLE     �   CREATE TABLE public.bbo_credit (
    id integer NOT NULL,
    userid integer,
    point integer,
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updateat timestamp without time zone
);
    DROP TABLE public.bbo_credit;
       public         heap r       postgres    false            �            1259    16479    bbo_credit_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_credit_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public.bbo_credit_id_seq;
       public               postgres    false    234            �           0    0    bbo_credit_id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public.bbo_credit_id_seq OWNED BY public.bbo_credit.id;
          public               postgres    false    233            �            1259    16446    bbo_feedback    TABLE     �   CREATE TABLE public.bbo_feedback (
    id integer NOT NULL,
    userid integer,
    rating integer,
    comment character varying(500),
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
     DROP TABLE public.bbo_feedback;
       public         heap r       postgres    false            �            1259    16445    bbo_feedback_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_feedback_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public.bbo_feedback_id_seq;
       public               postgres    false    228            �           0    0    bbo_feedback_id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE public.bbo_feedback_id_seq OWNED BY public.bbo_feedback.id;
          public               postgres    false    227            �            1259    16427    bbo_filters    TABLE     �   CREATE TABLE public.bbo_filters (
    id integer NOT NULL,
    question character varying(1000),
    displayorder integer,
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updateat timestamp without time zone
);
    DROP TABLE public.bbo_filters;
       public         heap r       postgres    false            �            1259    16426    bbo_filters_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_filters_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public.bbo_filters_id_seq;
       public               postgres    false    226            �           0    0    bbo_filters_id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public.bbo_filters_id_seq OWNED BY public.bbo_filters.id;
          public               postgres    false    225            �            1259    16456    bbo_notification    TABLE       CREATE TABLE public.bbo_notification (
    id integer NOT NULL,
    notification character varying(250),
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    status boolean,
    updateat timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
 $   DROP TABLE public.bbo_notification;
       public         heap r       postgres    false            �            1259    16455    bbo_notification_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_notification_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public.bbo_notification_id_seq;
       public               postgres    false    230            �           0    0    bbo_notification_id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE public.bbo_notification_id_seq OWNED BY public.bbo_notification.id;
          public               postgres    false    229            �            1259    16418    bbo_questionanalyst    TABLE     �   CREATE TABLE public.bbo_questionanalyst (
    id integer NOT NULL,
    questionpattern character varying(1000),
    mainkey character varying(100),
    askedcount integer
);
 '   DROP TABLE public.bbo_questionanalyst;
       public         heap r       postgres    false            �            1259    16417    bbo_questionanalyst_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_questionanalyst_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public.bbo_questionanalyst_id_seq;
       public               postgres    false    224            �           0    0    bbo_questionanalyst_id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE public.bbo_questionanalyst_id_seq OWNED BY public.bbo_questionanalyst.id;
          public               postgres    false    223            �            1259    16464    bbo_role    TABLE     �   CREATE TABLE public.bbo_role (
    id integer NOT NULL,
    rolename character varying(50),
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
    DROP TABLE public.bbo_role;
       public         heap r       postgres    false            �            1259    16463    bbo_role_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_role_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public.bbo_role_id_seq;
       public               postgres    false    232            �           0    0    bbo_role_id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE public.bbo_role_id_seq OWNED BY public.bbo_role.id;
          public               postgres    false    231            �            1259    16390    bbo_user    TABLE     �  CREATE TABLE public.bbo_user (
    id integer NOT NULL,
    telegramid integer,
    username character varying(100),
    joindate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    lastactive timestamp without time zone,
    isactive boolean DEFAULT true,
    roleid integer,
    password character varying(250),
    allownoti boolean,
    onchainid character varying(250)
);
    DROP TABLE public.bbo_user;
       public         heap r       postgres    false            �            1259    16389    bbo_user_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bbo_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public.bbo_user_id_seq;
       public               postgres    false    218            �           0    0    bbo_user_id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE public.bbo_user_id_seq OWNED BY public.bbo_user.id;
          public               postgres    false    217            �           2604    16402    bbo_chathistory chatid    DEFAULT     �   ALTER TABLE ONLY public.bbo_chathistory ALTER COLUMN chatid SET DEFAULT nextval('public.bbo_chathistory_chatid_seq'::regclass);
 E   ALTER TABLE public.bbo_chathistory ALTER COLUMN chatid DROP DEFAULT;
       public               postgres    false    220    219    220            �           2604    16412    bbo_chathistory_bk chatid    DEFAULT     �   ALTER TABLE ONLY public.bbo_chathistory_bk ALTER COLUMN chatid SET DEFAULT nextval('public.bbo_chathistory_bk_chatid_seq'::regclass);
 H   ALTER TABLE public.bbo_chathistory_bk ALTER COLUMN chatid DROP DEFAULT;
       public               postgres    false    222    221    222            �           2604    16483    bbo_credit id    DEFAULT     n   ALTER TABLE ONLY public.bbo_credit ALTER COLUMN id SET DEFAULT nextval('public.bbo_credit_id_seq'::regclass);
 <   ALTER TABLE public.bbo_credit ALTER COLUMN id DROP DEFAULT;
       public               postgres    false    234    233    234            �           2604    16449    bbo_feedback id    DEFAULT     r   ALTER TABLE ONLY public.bbo_feedback ALTER COLUMN id SET DEFAULT nextval('public.bbo_feedback_id_seq'::regclass);
 >   ALTER TABLE public.bbo_feedback ALTER COLUMN id DROP DEFAULT;
       public               postgres    false    227    228    228            �           2604    16430    bbo_filters id    DEFAULT     p   ALTER TABLE ONLY public.bbo_filters ALTER COLUMN id SET DEFAULT nextval('public.bbo_filters_id_seq'::regclass);
 =   ALTER TABLE public.bbo_filters ALTER COLUMN id DROP DEFAULT;
       public               postgres    false    225    226    226            �           2604    16459    bbo_notification id    DEFAULT     z   ALTER TABLE ONLY public.bbo_notification ALTER COLUMN id SET DEFAULT nextval('public.bbo_notification_id_seq'::regclass);
 B   ALTER TABLE public.bbo_notification ALTER COLUMN id DROP DEFAULT;
       public               postgres    false    229    230    230            �           2604    16421    bbo_questionanalyst id    DEFAULT     �   ALTER TABLE ONLY public.bbo_questionanalyst ALTER COLUMN id SET DEFAULT nextval('public.bbo_questionanalyst_id_seq'::regclass);
 E   ALTER TABLE public.bbo_questionanalyst ALTER COLUMN id DROP DEFAULT;
       public               postgres    false    223    224    224            �           2604    16467    bbo_role id    DEFAULT     j   ALTER TABLE ONLY public.bbo_role ALTER COLUMN id SET DEFAULT nextval('public.bbo_role_id_seq'::regclass);
 :   ALTER TABLE public.bbo_role ALTER COLUMN id DROP DEFAULT;
       public               postgres    false    232    231    232            �           2604    16393    bbo_user id    DEFAULT     j   ALTER TABLE ONLY public.bbo_user ALTER COLUMN id SET DEFAULT nextval('public.bbo_user_id_seq'::regclass);
 :   ALTER TABLE public.bbo_user ALTER COLUMN id DROP DEFAULT;
       public               postgres    false    217    218    218            m          0    16399    bbo_chathistory 
   TABLE DATA           q   COPY public.bbo_chathistory (chatid, userid, message, response, sentat, language_code, responsetime) FROM stdin;
    public               postgres    false    220   QX       o          0    16409    bbo_chathistory_bk 
   TABLE DATA           t   COPY public.bbo_chathistory_bk (chatid, userid, message, response, sentat, language_code, responsetime) FROM stdin;
    public               postgres    false    222   #c       {          0    16480 
   bbo_credit 
   TABLE DATA           L   COPY public.bbo_credit (id, userid, point, createdat, updateat) FROM stdin;
    public               postgres    false    234   @c       u          0    16446    bbo_feedback 
   TABLE DATA           N   COPY public.bbo_feedback (id, userid, rating, comment, createdat) FROM stdin;
    public               postgres    false    228   ]c       s          0    16427    bbo_filters 
   TABLE DATA           V   COPY public.bbo_filters (id, question, displayorder, createdat, updateat) FROM stdin;
    public               postgres    false    226   �d       w          0    16456    bbo_notification 
   TABLE DATA           Y   COPY public.bbo_notification (id, notification, createdat, status, updateat) FROM stdin;
    public               postgres    false    230   �g       q          0    16418    bbo_questionanalyst 
   TABLE DATA           W   COPY public.bbo_questionanalyst (id, questionpattern, mainkey, askedcount) FROM stdin;
    public               postgres    false    224   �g       y          0    16464    bbo_role 
   TABLE DATA           ;   COPY public.bbo_role (id, rolename, createdat) FROM stdin;
    public               postgres    false    232   �g       k          0    16390    bbo_user 
   TABLE DATA           �   COPY public.bbo_user (id, telegramid, username, joindate, lastactive, isactive, roleid, password, allownoti, onchainid) FROM stdin;
    public               postgres    false    218   `h       �           0    0    bbo_chathistory_bk_chatid_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('public.bbo_chathistory_bk_chatid_seq', 1, false);
          public               postgres    false    221            �           0    0    bbo_chathistory_chatid_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('public.bbo_chathistory_chatid_seq', 37, true);
          public               postgres    false    219            �           0    0    bbo_credit_id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public.bbo_credit_id_seq', 1, false);
          public               postgres    false    233            �           0    0    bbo_feedback_id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('public.bbo_feedback_id_seq', 12, true);
          public               postgres    false    227            �           0    0    bbo_filters_id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('public.bbo_filters_id_seq', 22, true);
          public               postgres    false    225            �           0    0    bbo_notification_id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public.bbo_notification_id_seq', 1, false);
          public               postgres    false    229            �           0    0    bbo_questionanalyst_id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('public.bbo_questionanalyst_id_seq', 1, false);
          public               postgres    false    223            �           0    0    bbo_role_id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('public.bbo_role_id_seq', 6, true);
          public               postgres    false    231            �           0    0    bbo_user_id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('public.bbo_user_id_seq', 3, true);
          public               postgres    false    217            �           2606    16416 *   bbo_chathistory_bk bbo_chathistory_bk_pkey 
   CONSTRAINT     l   ALTER TABLE ONLY public.bbo_chathistory_bk
    ADD CONSTRAINT bbo_chathistory_bk_pkey PRIMARY KEY (chatid);
 T   ALTER TABLE ONLY public.bbo_chathistory_bk DROP CONSTRAINT bbo_chathistory_bk_pkey;
       public                 postgres    false    222            �           2606    16407 $   bbo_chathistory bbo_chathistory_pkey 
   CONSTRAINT     f   ALTER TABLE ONLY public.bbo_chathistory
    ADD CONSTRAINT bbo_chathistory_pkey PRIMARY KEY (chatid);
 N   ALTER TABLE ONLY public.bbo_chathistory DROP CONSTRAINT bbo_chathistory_pkey;
       public                 postgres    false    220            �           2606    16486    bbo_credit bbo_credit_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY public.bbo_credit
    ADD CONSTRAINT bbo_credit_pkey PRIMARY KEY (id);
 D   ALTER TABLE ONLY public.bbo_credit DROP CONSTRAINT bbo_credit_pkey;
       public                 postgres    false    234            �           2606    16454    bbo_feedback bbo_feedback_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY public.bbo_feedback
    ADD CONSTRAINT bbo_feedback_pkey PRIMARY KEY (id);
 H   ALTER TABLE ONLY public.bbo_feedback DROP CONSTRAINT bbo_feedback_pkey;
       public                 postgres    false    228            �           2606    16435    bbo_filters bbo_filters_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY public.bbo_filters
    ADD CONSTRAINT bbo_filters_pkey PRIMARY KEY (id);
 F   ALTER TABLE ONLY public.bbo_filters DROP CONSTRAINT bbo_filters_pkey;
       public                 postgres    false    226            �           2606    16462 &   bbo_notification bbo_notification_pkey 
   CONSTRAINT     d   ALTER TABLE ONLY public.bbo_notification
    ADD CONSTRAINT bbo_notification_pkey PRIMARY KEY (id);
 P   ALTER TABLE ONLY public.bbo_notification DROP CONSTRAINT bbo_notification_pkey;
       public                 postgres    false    230            �           2606    16425 ,   bbo_questionanalyst bbo_questionanalyst_pkey 
   CONSTRAINT     j   ALTER TABLE ONLY public.bbo_questionanalyst
    ADD CONSTRAINT bbo_questionanalyst_pkey PRIMARY KEY (id);
 V   ALTER TABLE ONLY public.bbo_questionanalyst DROP CONSTRAINT bbo_questionanalyst_pkey;
       public                 postgres    false    224            �           2606    16470    bbo_role bbo_role_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public.bbo_role
    ADD CONSTRAINT bbo_role_pkey PRIMARY KEY (id);
 @   ALTER TABLE ONLY public.bbo_role DROP CONSTRAINT bbo_role_pkey;
       public                 postgres    false    232            �           2606    16397    bbo_user bbo_user_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public.bbo_user
    ADD CONSTRAINT bbo_user_pkey PRIMARY KEY (id);
 @   ALTER TABLE ONLY public.bbo_user DROP CONSTRAINT bbo_user_pkey;
       public                 postgres    false    218            �           2606    16498    bbo_user uq_telegramid 
   CONSTRAINT     W   ALTER TABLE ONLY public.bbo_user
    ADD CONSTRAINT uq_telegramid UNIQUE (telegramid);
 @   ALTER TABLE ONLY public.bbo_user DROP CONSTRAINT uq_telegramid;
       public                 postgres    false    218            �           2606    16499 #   bbo_chathistory fk_user_chathistory    FK CONSTRAINT     �   ALTER TABLE ONLY public.bbo_chathistory
    ADD CONSTRAINT fk_user_chathistory FOREIGN KEY (userid) REFERENCES public.bbo_user(telegramid);
 M   ALTER TABLE ONLY public.bbo_chathistory DROP CONSTRAINT fk_user_chathistory;
       public               postgres    false    4803    220    218            �           2606    16504 )   bbo_chathistory_bk fk_user_chathistory_bk    FK CONSTRAINT     �   ALTER TABLE ONLY public.bbo_chathistory_bk
    ADD CONSTRAINT fk_user_chathistory_bk FOREIGN KEY (userid) REFERENCES public.bbo_user(telegramid);
 S   ALTER TABLE ONLY public.bbo_chathistory_bk DROP CONSTRAINT fk_user_chathistory_bk;
       public               postgres    false    222    218    4803            �           2606    16514    bbo_credit fk_user_credit    FK CONSTRAINT     �   ALTER TABLE ONLY public.bbo_credit
    ADD CONSTRAINT fk_user_credit FOREIGN KEY (userid) REFERENCES public.bbo_user(telegramid);
 C   ALTER TABLE ONLY public.bbo_credit DROP CONSTRAINT fk_user_credit;
       public               postgres    false    234    218    4803            �           2606    16509    bbo_feedback fk_user_feedback    FK CONSTRAINT     �   ALTER TABLE ONLY public.bbo_feedback
    ADD CONSTRAINT fk_user_feedback FOREIGN KEY (userid) REFERENCES public.bbo_user(telegramid);
 G   ALTER TABLE ONLY public.bbo_feedback DROP CONSTRAINT fk_user_feedback;
       public               postgres    false    4803    228    218            �           2606    16492    bbo_user fk_user_role    FK CONSTRAINT     v   ALTER TABLE ONLY public.bbo_user
    ADD CONSTRAINT fk_user_role FOREIGN KEY (roleid) REFERENCES public.bbo_role(id);
 ?   ALTER TABLE ONLY public.bbo_user DROP CONSTRAINT fk_user_role;
       public               postgres    false    232    4817    218            m   �
  x��XM���=�~E�&f~J�r�F0֋8���^ZdKj�d���h�[��1r���{�a~�����&%Q�����`FC5���^�z�Qx��Y>��0�)d#�������kӊZ	�2�e���4��m)����k՝����҉�骒7��N	gĮ1��M�ً0~OE�,�dfA<M���F57o^N���f�m���ϭ���_��/�(U���J�Y��:'���}k�)L%mҪ޵�����Ra�^uN7:��*��_��أu����-�<ȓ8��ñ��c�Ue���^|�Y'6�ᘍ:��>Hv�Q����b��q��ER�f�0�&�����7����x��x�Z���_b��>8�C��#=�����ӻ�7����_�Ŗ�Z<���<^b�K�q�Hg�t��i6XO�y��rr/d-�i�#���:μO"�A����vpFSpm2_$i�ɔ,F�`M��g�~�?����"��l��P.�i0��0K�Ț���x�V�v�dI���^ �؛V�U��:�:�,�z_��T���O��^<f-�ҫ�D��J��^a�� |�iYa}�Mɨ���#w�%��>��Fl�b��U�'�9zr�s������u�hw��[|Dͼ�QKM)^j�ʔZY�Wm�ȴ\#x��|y���:^�0�a$YG\ �4H�I4Çy:��t:�ΙX�������G�vm�����W��R�ZS���BVr�+xC~�tr�;0ڂ\�]�Ҋ�iW�9|�����Ľ��$��ɬ_��:	�Ы�v��i���1�,��i���;b+MV��<Ŭ%��>��8GƏ�Z);e�W�,�3��LgA4� ��PE�7[��d*�犲���T�ֺВ�����{t`I�[Bw���p� L�Z�Z:d�~[��>J�kJp�#�(1?tʒ� �����m%Վ[^��B�� ��q��Z��h�p�g]_]#��:��:
��@/����iM���@���Ȓ 2����%hC��eAZ\p��o^C�4�g�V��[	�o�����6x�|f��@�ŵ��"L�0JS�7q��$?5�ܯأ:��b�!���}NyaFҳ��M�� ���}��*�Kz�DA�jj����Yw-ל�o�-��R5���˗��m�%[g�"1s��Â��^Z*���$jh��Xz�Qo����*�ǈ�8j@qx�TY�&��S���-��8�sued���l���� `W9���X�߫�jdu�2�2�G-��#��
u��F�Sjg=ْ �ἣ��7���ӊ$V�Q(������Vlq����s!�V�j
/��$�5�;�֙1�$�'a̲4�}M�l:��O��}�/ �J�g�^֔_��#SD��M��JTc+�C��"UE���'��"(qXSUr�C�ʙ�bl���kBd�bc�t%=�$�2���׼8%]�2��fAM��C�Y��^A��ć�ֱ�Z��k[aa�gi����<��#�zz�#��񟍨�F�spK�����N=�c��� ���|ޓRR�<��8�1rA�J�����@k���&�h��� I�9��EA�M⑾�a�8�N 1��\��y!=�=^$�c��a��dAgF��7�J�n��
�k��~���l��l'�,H�hL<� |y>�g�F�7t�Ev�j1?�4s����p��n��R^+p���缐KD,~�4� �]U��y�T;xS�e(�T\����� Ѓg��0��v�5�UΒJ��)�Tϔ�A�U4��'A��5��Z��k��p�<���A�WUY`�G�t�Aڢ�1B��G�dt����E7�צ}w;���*�aۿ��Аݫ�h�Z�O��sMQ��G���ɋz���ߪj\'9ZD��$�z���,����>J��U����iy����Z>��Z1g�E�l�[2�d��ϩ�Fƕ~z�_'��������?�#=���!�s@PȐ��Vq���{���%��iNN�T��*n������i����<$�z�9�:��g�"��L)�.�"�����dD�����T���Z�vQdd��@4 �c�Xs�I��9�h<G��d9@������*���3��ԋ�|��J���MT
�Y6r�X�(J2�$��1I>�����w��ˁ�0�KL�Η��s�ז�9�=�>��G1O��_���u-�0p>��	�9�K�y��
�����$��zwJ�O�iwLG�:]o<yF"q�HS����}�$H�Iru�0l��ǵ��j>�TU<���?R�k������ZE����->8���vt��?�V(c��uW�ŒJ�-h���@t��_O��&���O�Y��y������w�a2��Z)����I8lKq�A���6ܗۮBs�������U�����q>g�% �|��>�lO����>� �&�CC�`RNS��ߧ���/{z�D@.rj�ג�)��-�I��#p�X�7h�>���������|yX�5�p��C-�v�)vC��׾n�p-�Sǋ�sQ�tWI-˖�qI�$R�F�&��1��[��Rړ&�=�]������6]n���<|ʠߝ&������4|�	��g�K�ZW�A��O��#�/L��_����2Ks���Q����C�Y�9�&a��f��
�L��J�a�j��rj�9�2��q�������]vH��/���	j�$]�{���7NW�Cb��ϩ��E<��tc;��	&��� 9��8      o      x������ � �      {      x������ � �      u   P  x���=n�0�g��@T��!��t���q$@l�	��;Eѩ['gL/�TN���R��@�{�"!ء��������njt3�3$	�e�'
��Ϛ�w%�U�~�e�m�k?7�=��\u���y�or�:}mV��t�O�:(dGH���E�"Zt���n߫�KCtF�6�,�P�"_���>����qA���E���E�W�%�h\4��z�A�1���>�C
M�:�U��_.)�n�j���CH�z<�ѮY�1!E�����uy��4�4��b&��3��sL�Ny�^ ��X�m&���H�.$�C��!j���3�i+n����u      s   �  x��U˒�0<����2�P�${I�\�"�1VaI.I^�����|IF2�"X�l-�==3�b�W�5��-�tS�����JS�ֆ�#~���qB+*,��@�r��#i�N��x�0�$��f��l����� %���2-�:�iFr���(W9-��J�+u�ϒ�5�I�	����,�	����dt��c�f�Ɋ���&8%}[���"�n�i;k�%�+2����5�`�%PɅ�p��Ef�@�3�ǩSk�r�0�FXĭ��3�U��
���6��\�$�P˨�g��Q	Yɕ�rE���,	��:Õa�駮� ��v�ޓm�]ן�xqU����:,��U�u��9z���빢�½[���g�����s��7H9�����u��,
pW��/�i+�q�\��9&��{xe����y�|0BFD���&zLļ���L(y��]�̀w:γhl������ _��Ɂ(Է|���*)�s��㢲�y%v��+OR6J��?-^B���s�x��	�bO�sg�
 ���"��(-���c�8���}c��KFl�&�4�/"�z����o�9l�s���)2��[��"G�m�z�Xj���#o��<�ap�����&~�oJrQ`�g���1�n+��1�PA��*�����->eW��ބ�u,��=�w_D�ޅ8��M�\�'��SU�vx>�> ���a��7�s��4�v�      w      x������ � �      q      x������ � �      y   q   x�}�1�0D�z�����q��P���)."a�B�?]J�O�C�s]�Bct�R��GK�� �o�Z�����Q��5:���rO�0:�y�u��RZ�r�ʧ5{xU�ub->      k   �   x�}�K
�@�=��2�g����x�l�$��,���F��W�$5���4#k�T�TDY�,��z�݀�ݽ�<Y��A��n�U�5[Ŝ�g�����h*j���7�ƕ1���M�q���[��I26ї��HJ���^1�ȫZW�x>�����Bx��:u     