package jg.rpg.test.uitls;

import static org.junit.Assert.*;

import org.dom4j.DocumentException;
import org.junit.Test;

import jg.rpg.common.manager.DefEntityMgr;
import jg.rpg.exceptions.InitException;

public class TestTaskMgr {

	@Test
	public void testLoading() throws InitException, DocumentException {
		DefEntityMgr.getInstance().init();
	}
}